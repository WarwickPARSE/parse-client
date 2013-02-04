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
        private static double getBoundingBoxVolume(double xmin, double xmax, double ymin, double ymax, double zmin, double zmax)
        {
            return ((xmax - xmin) * (ymax - ymin) * (zmax - zmin));
        }

        private static double volume0thApprox(KdTree.KDTree pctree)
        {
            double xmin = pctree.getxMin();
            double xmax = pctree.getxMax();
            double ymin = pctree.getyMin();
            double ymax = pctree.getyMax();
            double zmin = pctree.getzMin();
            double zmax = pctree.getzMax();
            double volume = getBoundingBoxVolume(xmin,xmax,ymin,ymax,zmin,zmax);
            //volume = volume * ?;
            return volume;
        }

        //only works on an amorphus blob
        public static double volume1stApprox(KdTree.KDTree pctree)
        {
            double zmin = pctree.getzMin();
            double zmax = pctree.getzMax();
            double increment = 0.01;//pctree.getIncrement();
            double volume = 0;

            for (double i = zmin; i <= zmax; i = i + increment)
            {
                List<Point3D> plane = pctree.getAllPointsAt(i);
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
