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
        public static Point3D p2 = new Point3D(0, 7, 0);
        public static Point3D p3 = new Point3D(1, 6, 0);
        public static Point3D p4 = new Point3D(2, 4, 0);
        public static Point3D p5 = new Point3D(3, 0, 0);
        public static Point3D[] p = { p1, p2, p3, p4, p5 };
        public static List<Point3D> testList = new List<Point3D>(p);
        
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
        public static double volume1stApprox(PointCloud pc)//KdTree.KDTree pctree)
        {
            double zmin = pc.getzMin();
            double zmax = pc.getzMax();
            double increment = 0.01;//pctree.getIncrement();
            double volume = 0;

            for (double i = zmin; i <= zmax; i = i + increment)
            {
                List<Point3D> plane = testList//pctree.getAllPointsAt(i);
                plane.Add(plane[0]); //a list eating its own head, steve matthews would be proud

                double innerVolume = 0;

                for (int j = 0; j < plane.Count - 1; j++)
                {
                    innerVolume = innerVolume + ((plane[j].X * plane[j + 1].Y) - (plane[j + 1].X * plane[j].Y));
                }

                innerVolume = Math.Abs(innerVolume / 2);
                //innerVolume = innerVolume * ?;

                volume = volume + innerVolume;
            }

            //volume = volume * ?;
            return volume;
        }

        private static double calculateVolume(KdTree.KDTree pctree)
        {
            return volume0thApprox(pctree);
        }
    }
}
