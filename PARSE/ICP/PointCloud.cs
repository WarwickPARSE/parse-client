using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE.ICP
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

        /// <summary>
        /// Constructor for when just the arrays of x, y and z coordinates are provided.
        /// The resolution will be assumed to be 640 x 480
        /// </summary>
        /// <param name="x">array of x depth values</param>
        /// <param name="y">array of y depth values</param>
        /// <param name="z">array of z depth values</param>
        public PointCloud(int[] x, int[] y, int[] z) {
            this.x = x;
            this.y = y;
            this.z = y;
            this.width = 640;
            this.height = 480;
        }

        /// <summary>
        /// Constructor for when just the arrays of x, y and z coordinates are provided.
        /// The resolution will be assumed to be 640 x 480
        /// </summary>
        /// <param name="x">array of x depth values</param>
        /// <param name="y">array of y depth values</param>
        /// <param name="z">array of z depth values</param>
        /// <param name="pixels">the number of pixels (columns)</param>
        /// <param name="rows">the number of rows of pixels</param>
        public PointCloud(int[] x, int[] y, int[] z, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.z = y;
            this.width = width;
            this.height = height;
        }
        

    }
}
