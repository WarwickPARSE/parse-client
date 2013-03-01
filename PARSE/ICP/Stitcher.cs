using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE.ICP
{
    /// <summary>
    /// Abstract class that can be extended by point cloud stitching algorithms
    /// </summary>
    public abstract class Stitcher
    {
        //has the stitch method been called? This should default to false
        public bool processComplete = false; 

        //add a point cloud into the stitcher
        public abstract void add(PointCloud pc);

        //add a list of point clouds into the stitcher
        public abstract void add(List<PointCloud> pc);

        //this is where the stitching process takes place. This can be replalced by a dummy for on-the-fly stitching
        public abstract void stitch();

        /// <summary>
        /// Is the stitcher ready to return a result? 
        /// </summary>
        /// <returns>Readiness of the stitched point cloud</returns>
        public bool resultReady() {
            return this.processComplete; 
        }

        //returns the result of the stitching (PointCloud)
        public abstract PointCloud getResult();

        //returns the constituent parts of the stitching (PointCloud)
        public abstract List<PointCloud> getResultList();
    }
}
