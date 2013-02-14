using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;

namespace PARSE
{
    public class LimbCalculator
    {

        //provide depth z at a given joint co-ordinate and a pointcloud with that particular information.
        public static double calculate(PointCloud pc, Dictionary <JointType, double[]> jointDepths)
        {

            //TODO: Refine this method based on user selection.
            foreach (KeyValuePair<JointType, double[]> points in jointDepths)
            {

                //xmin,xmax,ymin,ymax values
                double xmin = pc.getxMin();
                double xmax = pc.getxMax();
                double zmin = pc.getzMin();
                double zmax = pc.getzMax();

                //depth (with default)
                double depth = (pc.getyMax() - pc.getyMin()) / 2;

                //output
                String output = "";

                if (points.Key == JointType.HipCenter)
                {
                   // xmax = jointDepths[JointType.HipLeft][1];
                   // xmin = jointDepths[JointType.HipRight][1];
                   // ymin = jointDepths[JointType.HipLeft][2];
                   // ymax = jointDepths[JointType.HipRight][2];
                   // depth = jointDepths[JointType.HipCenter][0];

                   // System.Diagnostics.Debug.WriteLine("Measuring Waist...");
                }
                else if (points.Key == JointType.ElbowLeft)
                {
                    xmin = jointDepths[JointType.ShoulderLeft][1];
                    xmax = jointDepths[JointType.ElbowLeft][1];
                    zmin = jointDepths[JointType.ElbowLeft][2];
                    zmax = jointDepths[JointType.ShoulderLeft][2];
                    depth = jointDepths[JointType.ElbowLeft][0];

                    System.Diagnostics.Debug.WriteLine("Measuring the upper left arm...");
                }

                double[] limits = { Math.Abs(xmin), Math.Abs(zmin), Math.Abs(xmax), Math.Abs(zmax) };

                System.Diagnostics.Debug.WriteLine(xmin + "*" + xmax + "*" + zmin + "*" + zmax);

                List<Point3D> plane = pc.getKDTree().getAllPointsAt(depth, 0.05, limits);//0.05 might need changing
                if (plane.Count != 0)
                {
                    plane = PointSorter.rotSort(plane);
                    plane.Add(plane[0]); //a list eating its own head, steve matthews would be proud

                    double circum = 0;

                    for (int j = 0; j < plane.Count - 1; j++)
                    {
                        circum = circum + Math.Sqrt(Math.Pow((plane[j + 1].X - plane[j].X), 2) + Math.Pow((plane[j + 1].Y - plane[j].Y), 2));
                    }

                    Console.WriteLine("Circum Pre Multi: " + circum);
                    circum = UnitConvertor.convertPCM(circum);
                    Console.WriteLine("Circum: " + circum);
                    return circum;
                }
                else
                {
                    Console.WriteLine("FIBRE'S ARE FUSSED TO THE HEAD!!!");
                }
            }
            return -1;
        }
    }
}
