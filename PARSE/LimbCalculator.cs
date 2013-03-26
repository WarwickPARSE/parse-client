using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

using Microsoft.Kinect;

namespace PARSE
{
    static class LimbCalculator
    {

        private static double[] bounds = new double[6];
        private static double xmin;
        private static double ymin;
        private static double zmin;
        private static double xmax;
        private static double ymax;
        private static double zmax;
        private static PointCloud segmentedPointcloud;

        public static void calculateLimbBounds(PointCloud pc, Dictionary<String, double[]> jointDepths, String limb) {

            //Calculate limb bounds based on limb choice

            switch (limb) 
            {
                case "ARM_LEFT": 
                
                xmin = jointDepths["ShoulderLeft"][1];
                xmax = jointDepths["HandLeft"][1];

                ymax = jointDepths["ShoulderLeft"][2];
                ymin = jointDepths["HandLeft"][2];
                                
                zmin = pc.getzMin();
                zmax = pc.getzMax();

                bounds = new double[] { xmin, ymin, zmin, xmax, ymax, zmax };

                System.Diagnostics.Debug.WriteLine("Bounds:" + xmin + ", " + ymin + ", " + zmin + ", " + xmax + ", " + ymax + ", " + zmax);

                break;
                
                case "WAIST":

                xmin = jointDepths["HipRight"][1];
                xmax = jointDepths["HipLeft"][1];

                ymax = jointDepths["HipCenter"][2];
                ymin = jointDepths["HipLeft"][2];

                zmin = pc.getzMin();
                zmax = pc.getzMax();

                bounds = new double[] {xmin, ymin, zmin, xmax, ymax, zmax};

                System.Diagnostics.Debug.WriteLine("Bounds:" + xmin + ", " + ymin + ", " + zmin + ", " + xmax + ", " + ymax + ", " + zmax);

                break;
                
                default: break;
            }

            segmentedPointcloud = pc.getSubRegion(bounds);

            Tuple<List<List<Point3D>>, double> T = PlanePuller.pullAll(segmentedPointcloud,2);

            System.Diagnostics.Debug.WriteLine("Your arm is " + CircumferenceCalculator.calculate(T.Item1, 1) + "m");

        }

    }
}
