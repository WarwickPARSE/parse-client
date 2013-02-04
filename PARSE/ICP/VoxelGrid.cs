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

        /// <summary>
        /// Nothing flashy here. Just saves the point cloud into memory
        /// </summary>
        /// <param name="pcd">A point cloud</param>
        /// <param name="x">Number of x separations</param>
        /// <param name="y">Number of y separations</param>
        /// <param name="z">Number of z separations</param>
        VoxelGrid(PointCloud pcd, int x, int y, int z) {
            this.pcd = pcd; 
        }


    }
}
