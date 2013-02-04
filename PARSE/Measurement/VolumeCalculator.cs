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
        public static Point3D p1 = new Point3D(1, 1, 0);
        public static Point3D p2 = new Point3D(1, 3, 0);
        public static Point3D p3 = new Point3D(3, 1, 0);
        public static Point3D p4 = new Point3D(3, 3, 0);
        public static Point3D[] p = { p1, p2, p3, p4 };
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

        private static double volume1stApprox(KdTree.KDTree pctree)
        {
            double zmin = 1;//pctree.getZMin();
            double zmax = 1;//pctree.getZMax();
            double increment 1;//= pctree.getIncrement();
            double volume = 0;

            for (double i = zmin; i <= zmax; i = i + increment)
            {
                List<Point3D> plane = testList; //pctree.getAllPointsAt(i);
                
                //volume = volume + ?;
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
