using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE.ICP.Stitchers
{
    class vtkICP : Stitcher
    {
        List<PointCloud> pointClouds;
        List<PointCloud> txpointClouds;
        PointCloud pcd;

        /// <summary>
        /// Instantiates an empty point cloud, ready for us to dump some data
        /// </summary>
        public vtkICP() {
            //instantiate empty lists and point clouds 
            pointClouds = new List<PointCloud>();
            txpointClouds = new List<PointCloud>();
            pcd = new PointCloud();
        }

        public override void add(List<PointCloud> pc)
        {
            throw new NotImplementedException();
        }

        public override void add(PointCloud pc)
        {
            throw new NotImplementedException();
        }

        public override void stitch()
        {
            throw new NotImplementedException();
        }

        public override PointCloud getResult()
        {
            throw new NotImplementedException();
        }

        public override List<PointCloud> getResultList()
        {
            throw new NotImplementedException();
        }

    }
}
