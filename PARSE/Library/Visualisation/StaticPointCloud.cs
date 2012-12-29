using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Forms;

using HelixToolkit.Wpf;

using Microsoft.Kinect;

namespace PARSE
{
    class StaticPointCloud
    {

        //Constants
        private int     depthFrameWidth = 640;
        private int     depthFrameHeight = 480;
        private int     cx = 640 / 2;
        private int     cy = 480 / 2;
        private int     tooCloseDepth = 0;
        private int     tooFarDepth = 4095;
        private int     unknownDepth = -1;
        private double  scale = 0.001;
        private double  fxinv = 1.0 / 480;
        private double  fyinv = 1.0 / 480;
        private double  ddt = 200;

        //Geometry
        List<int[]>         depthClouds;
        int[]               rawDepth;
        Point3D[]           depthFramePoints;
        Point[]             textureCoordinates;

        //Texture
        BitmapSource        bs;
        DiffuseMaterial     imageMesh;

        KinectSensor        kinectSensor;


        public StaticPointCloud(List<int[]> dp)
        {

            this.bs = bs;
            this.depthClouds = dp;
            textureCoordinates = new Point[depthFrameHeight * depthFrameWidth];
            depthFramePoints = new Point3D[depthFrameHeight * depthFrameWidth];

            //sanity check for helix responsiveness
            //create texture coordinates
            //createTexture();
            render();

            //create mesh raw
            //this.BaseModel.Geometry = this.Model.Geometry;
            //this.BaseModel.Material = this.Model.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightGray));
            //this.BaseModel.Transform = new TranslateTransform3D(-1, -2, 1);

        }

        public void render()
        {
            //create depth coordinates

            for (int i = 0; i < this.depthClouds.Count; i++)
            {
                this.rawDepth = this.depthClouds[i];
                runDemoModel(i);
                createDepthCoords();

                switch (i)
                {
                    case 0:
                        this.Model.Geometry = createMesh();
                        this.Model.Material = this.Model.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightGray));
                        this.Model.Transform = new TranslateTransform3D(-1, -2, 1);
                        break;
                    case 1:
                        this.Model2.Geometry = createMesh();
                        this.Model2.Material = this.Model2.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightGray));
                        this.Model2.Transform = new TranslateTransform3D(0, -2, 1);
                        break;
                    case 2:
                        this.Model3.Geometry = createMesh();
                        this.Model3.Material = this.Model3.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightGray));
                        this.Model3.Transform = new TranslateTransform3D(1, -2, 1);
                        break;
                    case 3:
                        this.Model4.Geometry = createMesh();
                        this.Model4.Material = this.Model4.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightGray));
                        this.Model4.Transform = new TranslateTransform3D(2, -2, 1);
                        break;
                } 

            }

        }

        public void createTexture()
        {
           /* for (int a = 0; a < depthFrameHeight; a++)
            {
                for (int b = 0; b < depthFrameWidth; b++)
                {

                    //alignment issues with color image- to be fixed
                    //this.textureCoordinates[(a * depthFrameWidth) + b] = new Point((double)b / (depthFrameWidth), (double)a / (depthFrameHeight));

                    //this.textureCoordinates[(a * depthFrameWidth) + b] = new Point((double) b+30, (double)a+30);
                    this.textureCoordinates[(a * depthFrameWidth) + b] = (double)kinectSensor.CoordinateMapper.MapDepthPointToColorPoint(DepthImageFormat.Resolution320x240Fps30, (DepthImagePoint) this.rawDepth[1], ColorImageFormat.RgbResolution640x480Fps30);

                }
            }

            System.Diagnostics.Debug.WriteLine("Texture created: " + this.textureCoordinates.Length);*/

            for (int a = 0; a < depthFrameHeight; a++)
            {
                for (int b = 0; b < depthFrameWidth; b++)
                {

                    //alignment issues with color image- to be fixed
                    this.textureCoordinates[(a * depthFrameWidth) + b] 
                        = new Point(((double)b / (depthFrameWidth - 1)), (double)a / (depthFrameHeight - 1));

                    this.textureCoordinates[(a * depthFrameWidth) + b] = new Point((double) b+30, (double)a+30);

                }
            }

            System.Diagnostics.Debug.WriteLine("Texture created: " + this.textureCoordinates.Length);
        }

        public void createDepthCoords()
        {

            for (int iy = 0; iy < 480; iy++)
            {
                for (int ix = 0; ix < 640; ix++)
                {
                    int i = (iy * 640) + ix;

                    if (rawDepth[i] == unknownDepth || rawDepth[i] < tooCloseDepth || rawDepth[i] > tooFarDepth)
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

        //required components

        //imageMesh = new DiffuseMaterial(new ImageBrush(this.outputColorBitmap));
        //this.Model.Material = this.Model.BackMaterial = imageMesh;

        /// <summary>
        /// Sanity check.
        /// </summary>

        public void runDemoModel(int pos)
        {

            // Create a mesh builder and add a box to it
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddBox(new Point3D(0, 0, 1), 1, 2, 0.5);
            meshBuilder.AddBox(new Rect3D(0, 0, 1.2, 0.5, 1, 0.4));

            // Create a mesh from the builder (and freeze it)
            var mesh = meshBuilder.ToMesh(true);

            // Create some materials
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);

            switch(pos) {

                case 0: this.Model = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial }; break;
                case 1: this.Model2 = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial }; break;
                case 2: this.Model3 = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial }; break;
                case 3: this.Model4 = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial }; break;
            }

        }

        /// <summary>
        /// Gets or sets the sanity model.
        /// </summary>
        /// <value>The model.</value>

        public GeometryModel3D Model { get; set; }
        public GeometryModel3D BaseModel { get; set; }
        public GeometryModel3D Model2 { get; set; }
        public GeometryModel3D Model3 { get; set; }
        public GeometryModel3D Model4 { get; set; }
    }
}
