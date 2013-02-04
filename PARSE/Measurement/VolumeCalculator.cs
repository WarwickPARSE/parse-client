using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.CvEnum;
using System.Windows.Media.Media3D;

namespace PARSE
{
    public static class VolumeCalculator
    {
        public static Point3D p1 = new Point3D(0, 0, 0);
        public static Point3D p2 = new Point3D(0, 2, 0);
        public static Point3D p3 = new Point3D(2, 2, 0);
        public static Point3D p4 = new Point3D(2, 0, 0);
        public static Point3D p5 = new Point3D(10, 12, 0);
        public static Point3D p6 = new Point3D(10, 10, 0);
        public static Point3D p7 = new Point3D(12, 10, 0);
        public static Point3D p8 = new Point3D(12, 12, 0);
        public static Point3D[] p = { p1, p2, p3, p4, p5, p6, p7, p8 };
        public static List<Point3D> testList = new List<Point3D>(p);
        
        private static double getBoundingBoxVolume(double xmin, double xmax, double ymin, double ymax, double zmin, double zmax)
        {
            return ((xmax - xmin) * (ymax - ymin) * (zmax - zmin));
        }

        private static double volume0thApprox(KdTree.KDTree pctree)
        {
            double xmin = pctree.getXMin();
            double xmax = pctree.getXMax();
            double ymin = pctree.getYMin();
            double ymax = pctree.getYMax();
            double zmin = pctree.getZMin();
            double zmax = pctree.getZMax();
            double volume = getBoundingBoxVolume(xmin,xmax,ymin,ymax,zmin,zmax);
            //volume = volume * ?;
            return volume;
        }

        private static double euclidDistance(Point3D P1, Point3D P2)
        {
            return Math.Sqrt(Math.Pow(P1.X - P2.X, 2) + Math.Pow(P1.Y - P2.Y, 2) + Math.Pow(P1.Z - P2.Z, 2));
        }

        public static double volume1stApprox(KdTree.KDTree pctree)
        {
            double zmin = pctree.getZMin();
            double zmax = pctree.getZMax();
            double increment = pctree.getIncrement();
            double volume = 0;

            double guard = 5;

            for (double i = zmin; i <= zmax; i = i + increment)
            {
                Point3D[] plane = pctree.getAllPointsAt(i).ToArray();
                List<Point3D> poly1 = new List<Point3D>();
                List<Point3D> poly2 = new List<Point3D>();

                for (int j = 0; j < plane.Length - 1; j++)
                {
                    if (euclidDistance(plane[j], plane[j + 1]) < guard)
                    {
                        poly1.Add(plane[j]);
                    }
                    else
                    {
                        poly2.Add(plane[j]);
                    }
                }

                if (euclidDistance(plane[plane.Length - 1], plane[0]) < guard)
                {
                    poly1.Add(plane[plane.Length - 1]);
                }
                else
                {
                    poly2.Add(plane[plane.Length - 1]);
                }


                Point3D[] plane1 = poly1.ToArray();
                Point3D[] plane2 = poly2.ToArray();

                double innerVolume = 0;

                for (int j = 0; j < plane1.Length - 1; j++)
                {
                    innerVolume = innerVolume + ((plane1[j].X * plane1[j + 1].Y) - (plane1[j + 1].X * plane1[j].Y));
                }

                innerVolume = innerVolume + ((plane1[plane1.Length - 1].X * plane1[0].Y) - (plane1[0].X * plane1[plane1.Length - 1].Y));

                for (int j = 0; j < plane2.Length - 1; j++)
                {
                    innerVolume = innerVolume + ((plane2[j].X * plane2[j + 1].Y) - (plane2[j + 1].X * plane2[j].Y));
                }

                innerVolume = innerVolume + ((plane2[plane2.Length - 1].X * plane2[0].Y) - (plane2[0].X * plane2[plane2.Length - 1].Y));
                
                
                innerVolume = Math.Abs(innerVolume / 2);

                volume = volume + innerVolume;
            }

            //volume = volume * ?;
            return volume;
        }

        private static double calculateVolume(KdTree.KDTree pctree)
        {
            //read in kd tre
            return volume0thApprox(pctree);
        }

        private static double partitionKDTree(KdTree.KDTree pctree)
        {


            return 0;
        }

        private static double computeConvexHull(Object[] pclayer)
        {

            return 0;
        }

    }
}
