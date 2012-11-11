﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE
{
    class PointCloud
    {
        //temporary array data structures
        int[] x;
        int[] y;
        int[] z;

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
            this.x = x;
        }

        public void setY(int[] y) 
        {
            this.y = y;
        }

        public void setZ(int[] z) 
        {
            this.z = z;
        }

        //setter, for the lazy
        public void setXYZ(int[] x, int[] y, int[] z) 
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Converts the x, y, z info to points and dumps them into a list
        /// </summary>
        public void init() {
            //only proceed if the coordinates match up
            if (y.Length == x.Length && x.Length == z.Length)
            {
                //only proceed if we have the number of points that we expected
                if (x.Length == ((width * height)))
                {
                    for (int i = 0; i < (width * height);  i++)
                    {
                        PARSE.ICP.Point p = new PARSE.ICP.Point(x[i], y[i], z[i]);
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
