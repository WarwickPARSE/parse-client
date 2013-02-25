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
using KdTree;

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

        //max and min values for the data stored within the point cloud 
        public BitmapSource bs;
        double maxx = double.MinValue;
        double maxy = double.MinValue;
        double maxz = double.MinValue;
        double minx = double.MaxValue;
        double miny = double.MaxValue;
        double minz = double.MaxValue;


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
            if (bs != null)
            {
                Bitmap b = convertToBitmap(bs);
                setPoints(rawDepth, b);
            }
            else
            {
                setPoints(rawDepth);
            }
        }

        public PointCloud()
        {
            //parameterless constructor needed for serialization
            //this.points = new KdTree.KDTree(3);
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
                        double y = (cy - iy) * zz * fyinv;
                        double z = zz;

                        /*
                         * This is a cheeky bug fix that I cannot be proud of. I am not sure why it works, but it does...  
                         */
                     
                        
                        //check min values
                        if (x < minx) { minx = x; }
                        if (y < miny) { miny = y; }
                        if (z < minz) { minz = z; }

                        //check max values
                        if (x > maxx) { maxx = x; }
                        if (y > maxy) { maxy = y; }
                        if (z > maxz) { maxz = z; }
                        
                        //create a new point key
                        double[] pointKey = new double[3];

                        //set key
                        pointKey[0] = x;
                        pointKey[1] = y;
                        pointKey[2] = z;                        

                        Point3D poLoc = new Point3D(x, y, z);
                        PARSE.ICP.PointRGB po = new PARSE.ICP.PointRGB(poLoc, r[i], g[i], b[i]);

                        this.points.insert(pointKey, po);
                    }
                }
            }
            
        }

        /// <summary>
        /// Adds an existing point cloud into this point cloud 
        /// </summary>
        /// <param name="pc">The point cloud to add</param>
        public void addPointCloud(PointCloud pc) { 
            //retrieve the kd tree
            KdTree.KDTree kd = pc.getKDTree();

            //define a max and min point 
            //TODO: set these to proper max+min vals from the point cloud object 
            double[] minPoint = new double[3] { -100, -100, -100 };
            double[] maxPoint = new double[3] { 100, 100, 100 };

            //retrieve a list of all item in the tree
            Object[] points2 = kd.range(minPoint, maxPoint);

            //iterate over every point and jam it in this point cloud
            foreach (Object element in points2) {
                //create k,v pair from data extracted
                PARSE.ICP.PointRGB value = (PARSE.ICP.PointRGB)element;
                double[] key = new double[3] {value.point.X, value.point.Y, value.point.Z};

                //jam the data into the existing kd-tree
                int duplicates = 0;
                try {
                    this.points.insert(key, value);
                }
                catch (KeyDuplicateException) {
                    //ignore duplicates
                    duplicates++; 
                }

                //Console.WriteLine("There were " + duplicates + " duplicate keys in the tree");
            }
        }

        /// <summary>
        /// Returns all points from the kd tree
        /// </summary>
        /// <returns>The points from the kd tree</returns>
        public PARSE.ICP.PointRGB[] getAllPoints() {
            //max and min values 
            double[] minVal = new double[3]{ double.MinValue, double.MinValue, double.MinValue };
            double[] maxVal = new double[3]{ double.MaxValue, double.MaxValue, double.MaxValue };

            //pull objects from kd tree
            Object[] objects = points.range(minVal, maxVal);

            //create somewhere to jam all the points 
            PARSE.ICP.PointRGB[] retPoints = new PARSE.ICP.PointRGB[objects.Length];

            //filthy: typecast everything in turn
            int i = 0;
            foreach(Object pt in objects) {
                retPoints[i] = (PARSE.ICP.PointRGB)pt;
                i++;
            }

            return retPoints;
        }

        /// <summary>
        /// Translate the point cloud by a given value
        /// </summary>
        /// <param name="tx">Up to three co-ords</param>
        public void translate(double[] tx) 
        {
            if (tx.Length == 3) {
                //turn the transformation vector into and object
                Console.WriteLine("Translating");
                TranslateTransform3D translation = new TranslateTransform3D(tx[0], tx[1], tx[2]);

                //pull out the entire tree
                PARSE.ICP.PointRGB[] pts = this.getAllPoints();

                //create a new kd tree 
                KdTree.KDTree newPoints = new KdTree.KDTree(3);  
              
                //iterate over every point and translate + jam in new tree
                foreach(PARSE.ICP.PointRGB point in pts) {
                    
                    //perform the new translation which does appear to work.
                    Matrix3D mtx = new Matrix3D();
                    mtx.Translate(new Vector3D(tx[0], tx[1], tx[2]));

                    //complete translation
                    Point3D newPoint = mtx.Transform(point.point);

                    //check if the x, y and z max and min coords need updating
                    //check min values
                    if (newPoint.X < minx) { minx = newPoint.X; }
                    if (newPoint.Y < miny) { miny = newPoint.Y; }
                    if (newPoint.Z < minz) { minz = newPoint.Z; }

                    //check max values
                    if (newPoint.X > maxx) { maxx = newPoint.X; }
                    if (newPoint.Y > maxy) { maxy = newPoint.Y; }
                    if (newPoint.Z > maxz) { maxz = newPoint.Z; }                    

                    //jam into the tree 
                    double[] key = new double[3] { newPoint.X, newPoint.Y, newPoint.Z };
                    newPoints.insert(key, new PARSE.ICP.PointRGB(newPoint, point.r, point.g, point.b));
                    
                    //perform the old translation method which doesn't appear to work.
                    //point.point.Offset(tx[0], tx[1], tx[2]);
                    //double[] key = new double[3]{point.point.X, point.point.Y, point.point.Z};
                    //newPoints.insert(key, point);
                }

                //replace the old kd tree with the new one
                this.points = newPoints;
            }
            else { 
                //probably want to throw an exception here
            }
        }

        /// <summary>
        /// Rotates the point cloud by a given angle
        /// </summary>
        /// <param name="axis">The axis of rotation</param>
        /// <param name="angle">The angle to which te point cloud is to be rotated</param>
        public void rotate(double[] axis, double angle) 
        {
            if (!(axis.Length != 3)) {
                //centre of rotation 
                Point3D centre = new Point3D(axis[0], axis[1], axis[2]);

                //pull out the entire tree
                PARSE.ICP.PointRGB[] pts = this.getAllPoints();

                //create a new kd tree 
                KdTree.KDTree newPoints = new KdTree.KDTree(3);  

                //iterate over every point and translate + jam in new tree
                foreach (PARSE.ICP.PointRGB point in pts)
                {
                    //create rot matrix
                    Matrix3D mtx = new Matrix3D();
                    Quaternion q = new Quaternion(new Vector3D(0, 1, 0), angle);
                    mtx.RotateAt(q, centre);

                    //complete rotation
                    Point3D newPoint = mtx.Transform(point.point);

                    //jam into the tree 
                    double[] key = new double[3] { newPoint.X, newPoint.Y, newPoint.Z };
                    newPoints.insert(key, new PARSE.ICP.PointRGB(newPoint, point.r, point.g, point.b));
                }

                //replace the old kd tree with the new one
                this.points = newPoints;
            }
            else{
                //throw an exception and annoy Bernie in the process ;)
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

        public double getxMax() { return maxx; }
        public double getyMax() { return maxy; }
        public double getzMax() { return maxz; }
        public double getxMin() { return minx; }
        public double getyMin() { return miny; }
        public double getzMin() { return minz; }
    
    }
}
