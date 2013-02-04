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

        private static double volume0thDegree(KdTree.KDTree pctree)
        {
            double xmin = pctree.getXMin();
            double xmax = pctree.getXMax();
            double ymin = pctree.getYMin();
            double ymax = pctree.getYMax();
            double zmin = pctree.getZMin();
            double zmax = pctree.getZMax();
            return getBoundingBoxVolume(xmin,xmax,ymin,ymax,zmin,zmax);
        }
        
        private static double calculateVolume(KdTree.KDTree pctree)
        {
            //read in kd tre
            return 0;
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
