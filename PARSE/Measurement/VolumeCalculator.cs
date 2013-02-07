using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Emgu.CV.CvEnum;
using System.Windows.Media.Media3D;
using PARSE.ICP;

namespace PARSE
{
    public static class VolumeCalculator
    {
        /*public static Point3D p2 = new Point3D(0, 0, 0);
        public static Point3D p1 = new Point3D(0, 7, 0);
        public static Point3D p4 = new Point3D(1, 6, 0);
        public static Point3D p5 = new Point3D(2, 4, 0);
        public static Point3D p3 = new Point3D(3, 0, 0);
        public static Point3D[] p = { p1, p2, p3, p4, p5 };
        public static List<Point3D> testList = new List<Point3D>(p);*/
        
        private static double getBoundingBoxVolume(double xmin, double xmax, double ymin, double ymax, double zmin, double zmax)
        {
            return ((xmax - xmin) * (ymax - ymin) * (zmax - zmin));
        }

        private static double volume0thApprox(PointCloud pc)//KdTree.KDTree pctree)
        {
            double xmin = pc.getxMin();
            double xmax = pc.getxMax();
            double ymin = pc.getyMin();
            double ymax = pc.getyMax();
            double zmin = pc.getzMin();
            double zmax = pc.getzMax();
            double volume = getBoundingBoxVolume(xmin,xmax,ymin,ymax,zmin,zmax);
            //volume = volume * ?;
            return volume;
        }

        //only works on an amorphus blob
        public static double volume1stApprox(PointCloud pc)
        {
            double xmin = pc.getxMin();
            double xmax = pc.getxMax();
            double ymin = pc.getyMin();
            double ymax = pc.getyMax();
            double[] limits = { xmin, ymin, xmax, ymax };

            double zmin = pc.getzMin();
            double zmax = pc.getzMax();
            double increment = 0.01;
            double volume = 0;

            for (double i = zmin + (increment / 2); i <= zmax - (increment / 2); i = i + increment)
            {
                List<Point3D> plane = pc.getKDTree().getAllPointsAt(i, increment / 2, limits);
                plane = rotSort(plane);
                plane.Add(plane[0]); //a list eating its own head, steve matthews would be proud

                double innerVolume = 0;

                for (int j = 0; j < plane.Count - 1; j++)
                {
                    innerVolume = innerVolume + ((plane[j].X * plane[j + 1].Y) - (plane[j + 1].X * plane[j].Y));
                }

                innerVolume = Math.Abs(innerVolume / 2);
                innerVolume = innerVolume * increment;

                volume = volume + innerVolume;
            }

            //volume = volume * ?;
            return volume;
        }
        
        public static double calculateVolume(PointCloud pc)
        {
            Console.WriteLine("Upper Bound on Patient Volume: "+volume0thApprox(pc));
            return 0;//volume1stApprox(pc);
        }

        private static int compareTwoPoints(Point3D a, Point3D b)
        {
            double aTanA, aTanB;

            //  Fetch the atans

            aTanA = Math.Atan2(a.Y, a.X);
            aTanB = Math.Atan2(b.Y, b.X);

            //  Determine next point in Clockwise rotation
            if (aTanA < aTanB) return -1;
            else if (aTanA > aTanB) return 1;
            return 0;
        }

        private static List<Point3D> rotSort(List<Point3D> input)
        {
            double xmax = double.MinValue;
            double xmin = double.MaxValue;
            double ymax = double.MinValue;
            double ymin = double.MaxValue;
            for (int i = 0; i < input.Count; i++)
            {
                if (input[i].X > xmax)
                {
                    xmax = input[i].X;
                }
                else if (input[i].X < xmin)
                {
                    xmin = input[i].X;
                }
                if (input[i].Y > ymax)
                {
                    ymax = input[i].Y;
                }
                else if (input[i].Y < ymin)
                {
                    ymin = input[i].Y;
                }
            }

            Point3D center = new Point3D((xmax - xmin) / 2, (ymax - ymin) / 2, 0);

            for (int i = 0; i < input.Count; i++)
            {
                input[i] = new Point3D(input[i].X - center.X, input[i].Y - center.Y, 0);
            }

            input.Sort(compareTwoPoints);

            for (int i = 0; i < input.Count; i++)
            {
                input[i] = new Point3D(input[i].X + center.X, input[i].Y + center.Y, 0);
            }

            return input;

        }
    }
}
