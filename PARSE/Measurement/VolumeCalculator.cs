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
        private const double pctorwtransform = 0.123195634;//to be determined experimentally
        
        private static double getBoundingBoxVolume(double xmin, double xmax, double ymin, double ymax, double zmin, double zmax)
        {
            return ((xmax - xmin) * (ymax - ymin) * (zmax - zmin));
        }

        private static double volume0thApprox(PointCloud pc)
        {
            double xmin = pc.getxMin();
            double xmax = pc.getxMax();
            double ymin = pc.getyMin();
            double ymax = pc.getyMax();
            double zmin = pc.getzMin();
            double zmax = pc.getzMax();
            double volume = getBoundingBoxVolume(xmin,xmax,ymin,ymax,zmin,zmax);
            Console.WriteLine("Volume Pre Multi: " + volume);
            volume = volume * pctorwtransform;
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
                if (plane.Count != 0)
                {
                    plane = PointSorter.rotSort(plane);
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
            }
            Console.WriteLine("Volume Pre Multi: " + volume);
            volume = volume * pctorwtransform;
            return volume;
        }
        
        public static double calculateVolume(PointCloud pc)
        {
            Console.WriteLine("Upper Bound on Patient Volume: " + volume0thApprox(pc));
            Console.WriteLine("Better Volume Patient Volume: " + volume1stApprox(pc));
            return 0;//volume1stApprox(pc);
        }
    }
}
