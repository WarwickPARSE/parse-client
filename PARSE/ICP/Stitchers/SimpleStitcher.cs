using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE.ICP
{
    /// <summary>
    /// This is basically ripped from bernie's code
    /// </summary>
    public class SimpleStitcher : Stitcher
    {
        List<PointCloud> pointClouds;
        PointCloud pcd;

        /// <summary>
        /// Instantiates an empty point cloud, ready for us to input some data
        /// </summary>
        public SimpleStitcher() {
            //instantiate empty lists and point clouds 
            pointClouds = new List<PointCloud>();
            pcd = new PointCloud();
        }

        /// <summary>
        /// Add a point cloud into the list of point clouds
        /// </summary>
        /// <param name="pc">Input point cloud</param>
        public override void add(PointCloud pc) {
            this.pointClouds.Add(pc);
        }

        /// <summary>
        /// Add a list of point clouds to the list of point clouds
        /// </summary>
        /// <param name="pcs"></param>
        public override void add(List<PointCloud> pcs) {
            this.pointClouds = pcs;
            Console.WriteLine("There are " + pcs.Count);
        }

        /// <summary>
        /// Stitch the point clouds together 
        /// </summary>
        public override void stitch() {

            //change the state to complete
            processComplete = true; 
            
            //sanity check (this class can only handle up to four clouds without setting fire)
            if (!(pointClouds.Count > 4)) {

                //iterate over each of the point clouds
                int i = 1;
                foreach (PointCloud cloud in pointClouds) {
                    switch (i)
                    {
                        case 1:
                            //stick the cloud in the main point cloud 
                            pcd = cloud;
                            System.Diagnostics.Debug.WriteLine("Front face is now in the pointcloud");
                            System.Diagnostics.Debug.WriteLine("Pointcloud size now: " + pcd.getAllPoints().Length);
                            
                            break;
                        case 2:
                            //rotate + translate 
                            cloud.rotate(new double[] { 0, 1, 0 }, -90);
                            cloud.translate(new double[] { -1.5, 1.25, 0 });
                            
                            pcd.addPointCloud(cloud);
                            
                            System.Diagnostics.Debug.WriteLine("Left face is now in the pointcloud");
                            System.Diagnostics.Debug.WriteLine("Pointcloud size now: " + pcd.getAllPoints().Length);

                            break;
                        case 3:
                            //rotate + translate
                            cloud.rotate(new double[] { 0, 1, 0 }, -180);
                            cloud.translate(new double[] { 0, 2.5, 0 });
                            
                            pcd.addPointCloud(cloud);
                            
                            System.Diagnostics.Debug.WriteLine("Back face is now in the pointcloud");
                            System.Diagnostics.Debug.WriteLine("Pointcloud size now: " + pcd.getAllPoints().Length);

                            break;
                        case 4:
                            //rotate + translate
                            cloud.rotate(new double[] { 0, 1, 0 }, -270);
                            cloud.translate(new double[] { 1.5, 1.25, 0 });
                            
                            pcd.addPointCloud(cloud);
                            
                            System.Diagnostics.Debug.WriteLine("Right face is now in the pointcloud");
                            System.Diagnostics.Debug.WriteLine("Pointcloud size now: " + pcd.getAllPoints().Length);

                            break;
                        default:
                            //throw an exception
                            break;
                    }

                    //iterate our counter 
                    i++;
                }
            }

        }

        /// <summary>
        /// Return the result of the stitching
        /// </summary>
        /// <returns>The result of the stitching</returns>
        public override PointCloud getResult() {
            return pcd;
        }
    }
}
