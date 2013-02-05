using System;
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
using System.Xml.Serialization;
using System.IO;

using HelixToolkit.Wpf;

namespace PARSE
{
    public class PointCloud
    {
        //dodgy global variables (to be changed)
        //private int width;
        //private int height;

        //tree of points
        private KdTree.KDTree points;

        //private bool is_dense;                  //to be calculated

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
        //private double ddt = 200;

        //bitmap source, accessible for visualisation.
        public BitmapSource bs; 

        //geometry, accessible for visualisation.
        public int[] rawDepth;
        Point3D[] depthFramePoints;
        System.Windows.Point[] textureCoordinates;

        //percentage of opacity that we want the colour to be 
        private const int opacity = 100;

        //the following variables may or may not be defined, depending on future need
        //sensor_orientation
        //sensor_origin

        public PointCloud(BitmapSource bs, int[] rawDepth) 
        {
            this.bs = bs;
            this.rawDepth = rawDepth;

            textureCoordinates = new System.Windows.Point[depthFrameHeight * depthFrameWidth];
            depthFramePoints = new Point3D[depthFrameHeight * depthFrameWidth];

            this.points = new KdTree.KDTree(3);

            //convert bitmap stream into a format that is supported by the kd-tree method
            Bitmap b = convertToBitmap(bs);

            setPoints(rawDepth, b);
        }
     
        /// <summary>
        /// Converts a bitmap stream into a bitmap image 
        /// </summary>
        /// <param name="bs">A bitmap stream</param>
        /// <returns></returns>
        public Bitmap convertToBitmap(BitmapSource bs) {
            //Convert bitmap source to image
            MemoryStream outStream = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            //System.Drawing.Bitmap resultBitmap;

            //Convert model image
            enc.Frames.Add(BitmapFrame.Create(bs));
            enc.Save(outStream);
            System.Drawing.Bitmap modbm = new System.Drawing.Bitmap(outStream);

            return modbm; 
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

                        //set key
                        pointKey[0] = x;
                        pointKey[1] = y;
                        pointKey[2] = z;

                        this.points.insert(pointKey, c);
                    }
                }
            }
            
        }

        /// <summary>
        /// returns kd-tree representation of point cloud
        /// </summary>
        /// <param name="points"></param>
        public KdTree.KDTree getKDTree()
        {
            return this.points;
        }
    
    }
}
