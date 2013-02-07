using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE.ICP.KdTree
{
    /// <summary>
    /// This is basically ripped from bernie's code
    /// </summary>
    class SimpleStitcher : Stitcher
    {
        List<PointCloud> pointClouds;
        PointCloud pcd;

        /// <summary>
        /// Instantiates an empty point cloud, ready for us to input some data
        /// </summary>
        SimpleStitcher() {
            //instantiate empty lists and point clouds 
            pointClouds = new List<PointCloud>();
            pcd = new PointCloud();
        }

        /// <summary>
        /// Add a point cloud into the list of point clouds
        /// </summary>
        /// <param name="pc">Input point cloud</param>
        public void add(PointCloud pc) {
            this.pointClouds.Add(pc);
        }

        /// <summary>
        /// Stitch the point clouds together 
        /// </summary>
        public void stitch() {

            //change the state to complete
            processComplete = true; 

            //offset (x, y, z)
            double[] offset = new double[3] { 1, 1, 1 };

            //rotation vector 
            Vector3D rotationAxis = new Vector3D(0, 0, 0);

            //rotation angle
            double rotationAngle = 0.0;

            //sanity check (this class can only handle up to four clouds)
            if (!(pointClouds.Count > 4)) {
                //iterate over each of the point clouds
                for (int i = 0; i <= pointClouds.Count; i++) { 
                    
                    //each cloud has something different happen to it
                    switch (i) {
                        case 1: 
                            //set the offeset
                            offset = new double[3] { -0.5, -3.4, 1 };
                            rotationAxis = new Vector3D(0, 0, 1);
                            rotationAngle = 90;
                            
                            break;
                        case 2: 
                            break;
                        case 3: 
                            break;
                        case 4: 
                            break;
                        default:
                            //throw an exception
                            break;
                    }

                    //translate and rotate the point cloud 

                    //stick it in the existing point cloud 
                }
            }
            else { 
                //throw an exception of some description 
            }
        }


        /// <summary>
        /// Return the result of the stitching
        /// </summary>
        /// <returns>The result of the stitching</returns>
        public PointCloud getResult() {
            return null; 
        }
    }
}
