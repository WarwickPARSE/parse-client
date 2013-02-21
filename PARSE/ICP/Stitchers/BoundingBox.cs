using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE.ICP.Stitchers
{
    class BoundingBox : Stitcher
    {
        List<PointCloud> pointClouds;
        PointCloud pcd;

        /// <summary>
        /// Instantiates an empty point cloud, ready for us to dump some data
        /// </summary>
        public BoundingBox() {
            //instantiate empty lists and point clouds 
            pointClouds = new List<PointCloud>();
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

        /// <summary>
        /// Stitch the point clouds using the "bounding box" algorithm
        /// </summary>
        public override void stitch() { 
            //this algorithm requires four clouds, so we first check that there are four clouds available
            if (pointClouds.Count == 4) {
                
                //instantiate some initial variables
                double depth = 0;
                double rotationAngle = 0;
                double[] translationValue = new double[3];
                double[] rotationCentre = new double[3];

                //instantiate previous coord variables 
                double[] prevMin = new double[3];
                double[] prevMax = new double[3];

                //iterate over every cloud 
                int i = 0;
                foreach (PointCloud cloud in pointClouds) {

                    //it turns out that the same translation works in most cases 
                    translationValue = new double[3] { depth, 0, 0 };

                    //perform the rotation depending on which point cloud we are looking at 
                    switch (i) { 
                        case 0:
                            //this is nice, we don't need to do anything! 
                            rotationAngle = 0; 
                            break;
                        case 1:
                            //set the rotation to a fixed value 
                            rotationAngle = -90;

                            //calculate the centre of rotation 
                            rotationCentre = new double[3] { cloud.getxMax(), cloud.getyMin(), cloud.getzMax() };
                            break;
                        case 2:
                            //set the rotation to a fixed value 
                            rotationAngle = -180;

                            //calculate the centre of rotation (maxx',miny,maxz'+(maxx-minx))
                            rotationCentre = new double[3] { prevMax[0], cloud.getyMin(), prevMax[2] + (cloud.getxMax() - cloud.getxMin()) }; 
                            break;
                        case 3:
                            //set the rotation to a fixed value 
                            rotationAngle = -270;

                            rotationCentre = new double[3] { cloud.getxMax(), cloud.getyMin(), cloud.getzMax() };
                            break;
                        default:
                            //this should not occur... throw an exception 
                            break;
                    }

                    if (i != 0) {
                        cloud.rotate(rotationCentre, rotationAngle);
                        cloud.translate(translationValue); 
                    }

                    //stick the result into the point cloud 
                    if (i == 0) {
                        this.pcd = cloud;
                    }
                    else {
                        this.pcd.addPointCloud(cloud);
                    }

                    //store current values for the next iteration 
                    prevMin = new double[3]{cloud.getxMin(), cloud.getyMin(), cloud.getzMin()};
                    prevMax = new double[3]{cloud.getxMax(), cloud.getyMax(), cloud.getzMax()};

                    //calculate the depth 
                    depth = cloud.getzMax() - cloud.getzMin();

                    //increase iterator 
                    i++;
                }
            }
            else { 
                //thow an exception 
            }
        }

        /// <summary>
        /// Return the result of the stitching
        /// </summary>
        /// <returns>The result of the stitching</returns>
        public override PointCloud getResult()
        {
            return pcd;
        }

    }
}
