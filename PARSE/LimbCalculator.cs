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
        public static PointCloud segmentedPointcloud;

        public static Tuple<double,double,List<List<Point3D>>> calculateLimbBounds(PointCloud pc, Dictionary<String, double[]> jointDepths, int limb) {

            //Calculate limb bounds based on limb choice
            double finalCircum = 0.0;
            double numPlanes = 0.0;
            Tuple<List<List<Point3D>>, double> T = new Tuple<List<List<Point3D>>, double>(null, 0);

            switch (limb) 
            {
                case 1:
                //SHOULDERS (1)
                xmin = jointDepths["ShoulderLeft"][1];
                xmax = jointDepths["ShoulderRight"][1];

                ymax = jointDepths["ShoulderCenter"][2];
                ymin = jointDepths["ShoulderLeft"][2];

                zmin = pc.getzMin();
                zmax = pc.getzMax();

                bounds = new double[] { xmin, ymin, zmin, xmax, ymax, zmax };

                //translate bounds according to pointcloud data points

                System.Diagnostics.Debug.WriteLine("Bounds:" + xmin + ", " + ymin + ", " + zmin + ", " + xmax + ", " + ymax + ", " + zmax);

                break;

                case 2: 
                
                //ARM_LEFT (2)
                xmin = jointDepths["ShoulderLeft"][1];
                xmax = jointDepths["HandLeft"][1];

                ymax = jointDepths["ShoulderLeft"][2];
                ymin = jointDepths["HandLeft"][2];
                                
                zmin = pc.getzMin();
                zmax = pc.getzMax();

                bounds = new double[] { xmin, ymin, zmin, xmax, ymax, zmax };

                //translate bounds according to pointcloud data points

                System.Diagnostics.Debug.WriteLine("Bounds:" + xmin + ", " + ymin + ", " + zmin + ", " + xmax + ", " + ymax + ", " + zmax);

                break;

                case 3:

                //ARM_RIGHT (3)
                xmin = jointDepths["ShoulderRight"][1];
                xmax = jointDepths["HandRight"][1];

                ymax = jointDepths["ShoulderRight"][2];
                ymin = jointDepths["HandRight"][2];

                zmin = pc.getzMin();
                zmax = pc.getzMax();

                bounds = new double[] { xmin, ymin, zmin, xmax, ymax, zmax };

                //translate bounds according to pointcloud data points

                System.Diagnostics.Debug.WriteLine("Bounds:" + xmin + ", " + ymin + ", " + zmin + ", " + xmax + ", " + ymax + ", " + zmax);

                break;

                case 4:

                //CHEST(4)

                xmin = jointDepths["ShoulderRight"][1];
                xmax = jointDepths["ShoulderLeft"][1];

                ymax = jointDepths["ShoulderCenter"][2];
                ymin = jointDepths["Spine"][2];

                zmin = pc.getzMin();
                zmax = pc.getzMax();

                bounds = new double[] { xmin, ymin, zmin, xmax, ymax, zmax };

                //translate bounds according to pointcloud data points

                System.Diagnostics.Debug.WriteLine("Bounds:" + xmin + ", " + ymin + ", " + zmin + ", " + xmax + ", " + ymax + ", " + zmax);

                break;

                case 5:

                //WAIST(5)

                xmin = jointDepths["HipRight"][1];
                xmax = jointDepths["HipLeft"][1];

                ymax = jointDepths["HipCenter"][2];
                ymin = jointDepths["HipLeft"][2];

                zmin = pc.getzMin();
                zmax = pc.getzMax();

                bounds = new double[] {xmin, ymin, zmin, xmax, ymax, zmax};

                //translate bounds according to pointcloud data points

                System.Diagnostics.Debug.WriteLine("Bounds:" + xmin + ", " + ymin + ", " + zmin + ", " + xmax + ", " + ymax + ", " + zmax);

                break;

                case 6:

                //LEFT_LEG(6)

                xmin = jointDepths["HipCenter"][1];
                xmax = jointDepths["HipLeft"][1];

                ymax = jointDepths["HipLeft"][2];
                ymin = jointDepths["KneeLeft"][2];

                zmin = pc.getzMin();
                zmax = pc.getzMax();

                bounds = new double[] { xmin, ymin, zmin, xmax, ymax, zmax };

                //translate bounds according to pointcloud data points

                System.Diagnostics.Debug.WriteLine("Bounds:" + xmin + ", " + ymin + ", " + zmin + ", " + xmax + ", " + ymax + ", " + zmax);

                break;

                case 7:

                //RIGHT_LEG(7)

                xmin = jointDepths["HipRight"][1];
                xmax = jointDepths["HipCenter"][1];

                ymax = jointDepths["HipRight"][2];
                ymin = jointDepths["KneeRight"][2];

                zmin = pc.getzMin();
                zmax = pc.getzMax();

                bounds = new double[] { xmin, ymin, zmin, xmax, ymax, zmax };

                //translate bounds according to pointcloud data points

                System.Diagnostics.Debug.WriteLine("Bounds:" + xmin + ", " + ymin + ", " + zmin + ", " + xmax + ", " + ymax + ", " + zmax);

                break;
                
                default: break;
            }

            try
            {

                segmentedPointcloud = pc.getSubRegion(bounds);

                T = PlanePuller.pullAll(segmentedPointcloud, 2);
                finalCircum = CircumferenceCalculator.calculate(T.Item1, 1);
                numPlanes = T.Item2;

            }
            catch (Exception err)
            {

                System.Diagnostics.Debug.WriteLine("(Subregion): Subregion issue - " + err.ToString());

            }

            //results printed out before historyloader so we can inspect all is well
            System.Diagnostics.Debug.WriteLine("***Limb Circumference Results***");
            System.Diagnostics.Debug.WriteLine("Limb chosen: " + limb);
            System.Diagnostics.Debug.WriteLine("Circumference approx: " + finalCircum);
            System.Diagnostics.Debug.WriteLine("Number of planes: " + numPlanes);

            return new Tuple<double, double, List<List<Point3D>>>(finalCircum, numPlanes, T.Item1);

        }

        public static Tuple<double,double, double> convertToPCCoords(double x, double y, double z) {

            //centre points
            int cx = 640 / 2;
            int cy = 480 / 2;

            //invariant values
            double fxinv = 1.0 / 476;
            double fyinv = 1.0 / 476;

            //depth scaling
            double zz = z;

            double xNew = (cx - x) * zz * fxinv;
            double yNew = (cy - y) * zz * fyinv;

            return new Tuple<double, double, double>(xNew,yNew,z);

        }

    }
}
