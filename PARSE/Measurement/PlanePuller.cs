using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE
{
    static class PlanePuller
    {
        public const int planeNumber = 60;
        public const int sampleNumber = 7;

        /// <summary>
        /// returns an arbitary number (60) of planes, sliced height wise, which have been averaged, subsampled and clock sorted.
        /// </summary>
        /// <param name="pc">PointCloud</param>
        /// <param name="arbNumber">int</param>
        /// <returns>Tuple(List(List(Point3D)),double)</returns>
        public static Tuple<List<List<Point3D>>, double> pullAll(PointCloud pc)
        {
            return PlanePuller.pullAll(pc, planeNumber);
        }

        /// <summary>
        /// returns arbNumber many planes, sliced height wise, which have been averaged, subsampled and clock sorted.
        /// </summary>
        /// <param name="pc">PointCloud</param>
        /// <param name="arbNumber">int</param>
        /// <returns>Tuple(List(List(Point3D)),double)</returns>
        public static Tuple<List<List<Point3D>>,double> pullAll(PointCloud pc, int arbNumber)
        {
            double xmin = pc.getxMin();
            double xmax = pc.getxMax();
            double zmin = pc.getzMin();
            double zmax = pc.getzMax();
            double[] limits = { xmin, zmin, xmax, zmax };

            double ymin = pc.getyMin();
            double ymax = pc.getyMax();
            double increment = (ymax - ymin) / arbNumber;

            List<List<Point3D>> output = new List<List<Point3D>>();

            for (double i = ymin + (increment / 2); i <= ymax - (increment / 2); i = i + increment)
            {
                List<Point3D> plane = pc.getKDTree().getAllPointsAt(i, increment / 2, limits);

                plane = SubSampler.averageSubSample(plane, sampleNumber);
                plane = PointSorter.clockSort(plane);
                output.Add(plane);
            }
            
            return Tuple.Create(output,increment);
        }
    }
}
