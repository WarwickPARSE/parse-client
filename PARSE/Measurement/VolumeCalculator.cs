using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV.CvEnum;

namespace PARSE
{
    public static class VolumeCalculator
    {
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
            double zmin = pctree.getZMin();
            double zmax = pctree.getZMax();
            double increment = pctree.getIncrement();
            double volume = 0;

            for (double i = zmin; i <= zmax; i = i + increment)
            {
                List<> = pctree.getAllPointsAt(i);
                
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
