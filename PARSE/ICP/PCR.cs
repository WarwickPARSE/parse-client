using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE.ICP
{
    /// <summary>
    /// Point Cloud Registration algorithm based on Makadia et all 2006
    /// </summary>
    class PCR
    {
        PointCloud[] pointClouds;

        /// <summary>
        /// Constructor method for a point cloud registration class 
        /// </summary>
        /// <param name="pointClouds">An arbitrary number of point clouds</param>
        public PCR(params PointCloud[] pointClouds) {
            this.pointClouds = pointClouds; 


        }
        private void computeSurfaceNormal() {
        
        }

        private void generateOrientationHistogram() { 
        
        }


    }
}
