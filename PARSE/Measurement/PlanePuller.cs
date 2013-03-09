using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE
{
    static class PlanePuller
    {
        public const int number = 60;

        public static Tuple<List<List<Point3D>>,double> pullAll(PointCloud pc)
        {
            double xmin = pc.getxMin();
            double xmax = pc.getxMax();
            double zmin = pc.getzMin();
            double zmax = pc.getzMax();
            double[] limits = { xmin, zmin, xmax, zmax };

            double ymin = pc.getyMin();
            double ymax = pc.getyMax();
            double increment = (ymax - ymin) / number;

            List<List<Point3D>> output = new List<List<Point3D>>();

            for (double i = ymin + (increment / 2); i <= ymax - (increment / 2); i = i + increment)
            {
                List<Point3D> plane = pc.getKDTree().getAllPointsAt(i, increment / 2, limits);
                //plane = ConvexHullFinder.findHull(plane);
                output.Add(plane);
            }

            return Tuple.Create(output,increment);
        }
    }
}
