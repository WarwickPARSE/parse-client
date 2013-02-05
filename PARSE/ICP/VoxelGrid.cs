using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE.ICP
{
    /// <summary>
    /// Implementation of a voxel grid. Similar idea to a point cloud but contains metohds to downsample etc. 
    /// </summary>
    class VoxelGrid
    {
        PointCloud pcd;

        //division values
        int xSep;
        int ySep;
        int zSep; 

        //maximum values from the point cloud (for easy+efficient access)
        int xMax;
        int yMax;
        int zMax;

        /// <summary>
        /// Nothing flashy here. Just saves the point cloud into memory
        /// </summary>
        /// <param name="pcd">A point cloud</param>
        /// <param name="xSep">Number of x separations</param>
        /// <param name="ySep">Number of y separations</param>
        /// <param name="zSep">Number of z separations</param>
        VoxelGrid(PointCloud pcd, int xSep, int ySep, int zSep) {
            this.pcd = pcd;

            this.xSep = xSep;
            this.ySep = ySep;
            this.zSep = zSep;
        }

        public void getVoxel(int indexx, int indexy, int indexz) { 
        
        }
        
    }
}
