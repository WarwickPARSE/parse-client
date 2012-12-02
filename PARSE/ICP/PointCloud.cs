using System;
//using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Forms;

using HelixToolkit.Wpf;

namespace PARSE
{
    class PointCloud
    {
        //dodgy global variables (to be changed)
        private int width;
        private int height;

        //tree of points
        private KdTree.KDTree points;

        private bool is_dense;                  //to be calculated

        //depth constants
        private const int tooCloseDepth = 0;
        private const int tooFarDepth = 4095;
        private const int unknownDepth = -1;
        private const double scale = 0.001;             //for rendering 

        //dimension constants 
        private int depthFrameWidth = 640;
        private int depthFrameHeight = 480;

        //centre points (these will need changing at some point)
        private const int cx = 640 / 2;
        private const int cy = 480 / 2;

        //fiddle values (they make it work but I don't know why!)
        private const double fxinv = 1.0 / 476;
        private const double fyinv = 1.0 / 476;
        private double ddt = 200;

        //bitmap source, to be deprecated soon
        BitmapSource bs; 

        //geometry
        int[] rawDepth;
        Point3D[] depthFramePoints;
        System.Windows.Point[] textureCoordinates;

        //percentage of opacity that we want the colour to be 
        private const int opacity = 100;

        //the following variables may or may not be defined, depending on future need
        //sensor_orientation
        //sensor_origin

        /// <summary>
        /// Constructor for when just the arrays of x, y and z coordinates are provided.
        /// The resolution will be assumed to be 640 x 480
        /// </summary>
        public PointCloud() {
            this.width = 640;
            this.height = 480;

            //create a new 3d point cloud
            this.points = new KdTree.KDTree(3);
        }

        /// <summary>
        /// Constructor for when just the arrays of x, y and z coordinates are provided.
        /// The resolution will be assumed to be 640 x 480
        /// </summary>
        /// <param name="pixels">the number of pixels (columns)</param>
        /// <param name="rows">the number of rows of pixels</param>
        public PointCloud(int width, int height)
        {
            this.width = width;
            this.height = height;

            //create a new 3d point cloud 
            this.points = new KdTree.KDTree(3);
        }


        public PointCloud(BitmapSource bs, int[] rawDepth) 
        {
            this.bs = bs;
            this.rawDepth = rawDepth;

            textureCoordinates = new System.Windows.Point[depthFrameHeight * depthFrameWidth];
            depthFramePoints = new Point3D[depthFrameHeight * depthFrameWidth];

            this.legacy();

            //create mesh default 
            this.Model.Geometry = createMesh();
            this.Model.Material = this.Model.BackMaterial = new DiffuseMaterial(new ImageBrush(this.bs));
            this.Model.Transform = new TranslateTransform3D(1, -2, 1);

            //this is pointless if you are instantiating in this manner, but meh 
            this.points = new KdTree.KDTree(3);
        }

        /// <summary>
        /// Generates a point cloud with no colours
        /// </summary>
        /// <param name="rawDepth"></param>
        public void setPoints(int[] rawDepth) 
        {
            int[] r= new int [rawDepth.Length];
            int[] g = new int[rawDepth.Length];
            int[] b = new int[rawDepth.Length];

            //fill each colour element with an empty color
            for (int i=0; i<rawDepth.Length; i++) 
            {
                r[i] = 0;
                g[i] = 0;
                b[i] = 0;
            }
            
            //call this overloaded method
            setPoints(rawDepth, r, g, b);
        }

        //this may cause runtime errors - due to the implicit typecasting from byte to int, will need to test this further
        public void setPoints(int[] rawDepth, Bitmap image)
        {
            Rectangle rec = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData imageData = image.LockBits(rec, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            //get address of the first scanline 
            IntPtr ptr = imageData.Scan0;

            //basically width*height
            int bytes = imageData.Stride * image.Height;

            //create an array of bytes to hold the bmp
            byte[] rgbData = new byte[bytes];
            int[] r = new int[bytes / 3];
            int[] g = new int[bytes / 3];
            int[] b = new int[bytes / 3];

            //copy the rgb values into the array
            Marshal.Copy(ptr, rgbData, 0, bytes);

            int i = 0;
            int stride = imageData.Stride;

            //shove the image data into its place 
            for (int col = 0; col < imageData.Height; col++)
            {
                for (int row = 0; row < imageData.Width; row++) 
                {
                    b[i] = (rgbData[(col * stride) + (row * 3)]); 
                    g[i] = (rgbData[(col * stride) + (row * 3) + 1]);
                    r[i] = (rgbData[(col * stride) + (row * 3) + 2]);
                    i++; //this may cause an array out of bounds, must look at this 
                }
            }

            //now call the overloaded method 
            setPoints(rawDepth, r, g, b);
        }

        //this is not fully implemented as I don't know how colours are represented!
        //TODO: throw an exception if the rawdepth is not the same length as rgb
        public void setPoints(int[] rawDepth, int[] r, int[] g, int[] b) 
        {
            for (int iy = 0; iy < 480; iy++)
            {
                for (int ix = 0; ix < 640; ix++)
                {
                    int i = (iy * 640) + ix;

                    if (rawDepth[i] == unknownDepth || rawDepth[i] < tooCloseDepth || rawDepth[i] > tooFarDepth)
                    {
                        rawDepth[i] = -1;

                        //at the moment we seem to be deleting points that are too far away, this will need changing at some point
                        //this.depthFramePoints[i] = new Point3D();
                    }
                    else
                    {
                        double zz = rawDepth[i] * scale;
                        double x = (cx - ix) * zz * fxinv;
                        double y = zz;
                        double z = (cy - iy) * zz * fyinv;
                        //this.depthFramePoints[i] = new Point3D(x, y, z);

                        //create a new colour using the info given
                        System.Drawing.Color c = System.Drawing.Color.FromArgb(opacity, r[i], g[i], b[i]);

                        //create a new point key
                        double[] pointKey = new double[3];

                        this.points.insert(pointKey, c);
                    }
                }
            }
            
        }

        /***
         * Aids in the transition from Bernie's point cloud to this one 
         */
        public void legacy()
        {
            //sanity check for helix responsiveness 
            this.runDemoModel();

            //create the texture coords
            this.createTexture();

            //create the depth coords
            this.createDepthCoords();
        }
        
        /***
         * Creates tecture coordinates  
         */
        private void createTexture()
        {

            for (int a = 0; a < depthFrameHeight; a++)
            {
                for (int b = 0; b < depthFrameWidth; b++)
                {

                    //alignment issues - to be fixed.
                    this.textureCoordinates[a * depthFrameWidth + b]
                        = new System.Windows.Point((double)b / (depthFrameWidth - 1), (double)a / (depthFrameHeight - 1));
                }
            }

            System.Diagnostics.Debug.WriteLine("Texture created: " + this.textureCoordinates.Length);
        }

        /***
         * Creates depth coordinates, was private scope previously  
         */
        private void createDepthCoords()
        {

            for (int iy = 0; iy < 480; iy++)
            {
                for (int ix = 0; ix < 640; ix++)
                {
                    int i = (iy * 640) + ix;

                    if (rawDepth[i] == unknownDepth || rawDepth[i] < tooCloseDepth || rawDepth[i] > tooFarDepth || rawDepth[i] > 2500)
                    {
                        this.rawDepth[i] = -1;
                        this.depthFramePoints[i] = new Point3D();
                    }
                    else
                    {
                        double zz = this.rawDepth[i] * scale;
                        double x = (cx - ix) * zz * fxinv;
                        double y = zz;
                        double z = (cy - iy) * zz * fyinv;
                        this.depthFramePoints[i] = new Point3D(x, y, z);
                    }
                }
            }
        }

        public MeshGeometry3D createMesh()
        {
            var triangleIndices = new List<int>();
            for (int iy = 0; iy + 1 < depthFrameHeight; iy++)
            {
                for (int ix = 0; ix + 1 < depthFrameWidth; ix++)
                {
                    int i0 = (iy * depthFrameWidth) + ix;
                    int i1 = (iy * depthFrameWidth) + ix + 1;
                    int i2 = ((iy + 1) * depthFrameWidth) + ix + 1;
                    int i3 = ((iy + 1) * depthFrameWidth) + ix;

                    var d0 = this.rawDepth[i0];
                    var d1 = this.rawDepth[i1];
                    var d2 = this.rawDepth[i2];
                    var d3 = this.rawDepth[i3];

                    var dmax0 = Math.Max(Math.Max(d0, d1), d2);
                    var dmin0 = Math.Min(Math.Min(d0, d1), d2);
                    var dmax1 = Math.Max(d0, Math.Max(d2, d3));
                    var dmin1 = Math.Min(d0, Math.Min(d2, d3));

                    if (dmax0 - dmin0 < ddt && dmin0 != -1)
                    {
                        triangleIndices.Add(i0);
                        triangleIndices.Add(i1);
                        triangleIndices.Add(i2);
                    }

                    if (dmax1 - dmin1 < ddt && dmin1 != -1)
                    {
                        triangleIndices.Add(i0);
                        triangleIndices.Add(i2);
                        triangleIndices.Add(i3);
                    }
                }
            }

            return new MeshGeometry3D()
            {
                Positions = new Point3DCollection(this.depthFramePoints),
                TextureCoordinates = new System.Windows.Media.PointCollection(this.textureCoordinates),
                TriangleIndices = new Int32Collection(triangleIndices)
            };
        }

        /***
         * Some sanity check 
         */
        private void runDemoModel()
        {
            // Create a mesh builder and add a box to it
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddBox(new Point3D(0, 0, 1), 1, 2, 0.5);
            meshBuilder.AddBox(new Rect3D(0, 0, 1.2, 0.5, 1, 0.4));

            // Create a mesh from the builder (and freeze it)
            var mesh = meshBuilder.ToMesh(true);

            // Create some materials
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);

            this.Model = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial };
           // this.BaseModel = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial };
        }

        /// <summary>
        /// Gets or sets the sanity model.
        /// </summary>
        /// <value>The model.</value>

        public GeometryModel3D Model { get; set; }
        public GeometryModel3D BaseModel { get; set; }

        //
    }
}
