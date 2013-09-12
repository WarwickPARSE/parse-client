using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kitware.mummy;
using Kitware.VTK; 

namespace PARSE.ICP.Stitchers
{
    class vtkICP : Stitcher
    {
        List<PointCloud> pointClouds;
        List<PointCloud> txpointClouds;
        PointCloud pcd;

        vtkIterativeClosestPointTransform vtkICPAlgo; 
        vtkPoints sourceData;
        vtkCellArray sourcePoints; 
    
        /// <summary>
        /// Instantiates an empty point cloud, ready for us to dump some data
        /// </summary>
        public vtkICP() {
            //instantiate empty lists and point clouds 
            pointClouds = new List<PointCloud>();
            txpointClouds = new List<PointCloud>();
            pcd = new PointCloud();

            //instantiate vtk lists
            vtkICPAlgo = new vtkIterativeClosestPointTransform();
            sourceData = new vtkPoints();
            sourcePoints = new vtkCellArray(); 
        }

        public override void add(PointCloud pc)
        {
            //stick into the pointcloud list
            this.pointClouds.Add(pc);

            //if the point cloud list isn't empty, register against previous scan 
            if (this.pointClouds.Count != 0) {
                //register against previous scan

                //dump the points from the cloud into the points 
                
            }
        }

        public override void add(List<PointCloud> pc)
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
