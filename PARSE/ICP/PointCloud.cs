using System;
using System.Collections.Generic;
using System.Drawing; 
using System.Linq;
using System.Text;

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

        //centre points (these will need changing at some point)
        private const int cx = 640 / 2;
        private const int cy = 480 / 2;

        //fiddle values (they make it work but I don't know why!)
        private const double fxinv = 1.0 / 476;
        private const double fyinv = 1.0 / 476;

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

        /// <summary>
        /// Generates a point cloud with no colours
        /// </summary>
        /// <param name="rawDepth"></param>
        public void setPoints(int[] rawDepth) 
        {
            //opacity % 
            int transparency = 0;


            for (int i = 0; i < rawDepth.Length; i++) { 
                //generate null colours with unlimited transparency 
            }
        }

        //this is not fully implemented as I don't know how colours are represented!
        public void setPoints(int[] rawDepth, int[] rawColor) 
        {
            for (int iy = 0; iy < 480; iy++)
            {
                for (int ix = 0; ix < 640; ix++)
                {
                    int i = (iy * 640) + ix;

                    if (rawDepth[i] == unknownDepth || rawDepth[i] < tooCloseDepth || rawDepth[i] > tooFarDepth)
                    {
                        rawDepth[i] = -1;
                        //this.depthFramePoints[i] = new Point3D();
                    }
                    else
                    {
                        double zz = rawDepth[i] * scale;
                        double x = (cx - ix) * zz * fxinv;
                        double y = zz;
                        double z = (cy - iy) * zz * fyinv;
                        //this.depthFramePoints[i] = new Point3D(x, y, z);
                    }
                }
            }
            
        }
    }
}
