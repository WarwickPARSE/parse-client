using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE.ICP.Stitchers
{
    class AffineICP : Stitcher
    {
        List<PointCloud> pointClouds;
        List<PointCloud> txpointClouds;
        PointCloud pcd;

        /// <summary>
        /// Instantiates an empty point cloud, ready for us to dump some data
        /// </summary>
        public AffineICP() {
            //instantiate empty lists and point clouds 
            pointClouds = new List<PointCloud>();
            txpointClouds = new List<PointCloud>();
            pcd = new PointCloud();
        }

        /// <summary>
        /// Add a point cloud into the list of point clouds
        /// </summary>
        /// <param name="pc">Input point cloud</param>
        public override void add(PointCloud pc)
        {
            this.pointClouds.Add(pc);
        }

        /// <summary>
        /// Add a list of point clouds to the list of point clouds
        /// </summary>
        /// <param name="pcs"></param>
        public override void add(List<PointCloud> pcs)
        {
            this.pointClouds = pcs;
            Console.WriteLine("There are " + pcs.Count);
        }

        public override void stitch() { 
        
        }

        /// <summary>
        /// Return the result of the stitching
        /// </summary>
        /// <returns>The result of the stitching</returns>
        public override PointCloud getResult()
        {
            return pcd;
        }

        public override List<PointCloud> getResultList()
        {
            return txpointClouds;
        }
    }
}
