using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE
{
    class PointCloud
    {
        //temporary array data structures
        int[] xs;
        int[] ys;
        int[] zs;

        //dodgy global variables (to be changed)
        int width;
        int height;

        bool is_dense;                          //to be calculated

        List<PARSE.ICP.Point> points;

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

            this.points = new List<PARSE.ICP.Point>();
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

            this.points = new List<PARSE.ICP.Point>();
        }

        public void setX(int[] x) 
        {
            this.xs = x;
        }

        public void setY(int[] y) 
        {
            this.ys = y;
        }

        public void setZ(int[] z) 
        {
            this.zs = z;
        }

        //setter, for the lazy
        public void setXYZ(int[] x, int[] y, int[] z) 
        {
            this.xs = x;
            this.ys = y;
            this.zs = z;
        }

        /// <summary>
        /// Converts the x, y, z info to points and dumps them into a list
        /// </summary>
        public void init() {
            //only proceed if the coordinates match up
            if (ys.Length == xs.Length && xs.Length == zs.Length)
            {
                //only proceed if we have the number of points that we expected
                if (xs.Length == ((width * height) - 1))
                {
                    for (int i = 0; i < (width * height);  i++)
                    {
                        PARSE.ICP.Point p = new PARSE.ICP.Point(xs[i], ys[i], zs[i]);
                        points.Add(p);
                    }
                }
                else 
                {
                    //throw another kind of exception                 
                }
            }
            else 
            { 
                //throw some kind of exception 
            }
        }

        public int countPoints() 
        {
            return points.Count; 
        }

    }
}
