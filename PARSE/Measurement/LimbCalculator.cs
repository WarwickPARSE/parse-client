using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE
{
    public class LimbCalculator
    {
        public static double calculate(double depth, PointCloud pc)
        {
            double xmin = pc.getxMin();
            double xmax = pc.getxMax();
            double ymin = pc.getyMin();
            double ymax = pc.getyMax();
            double[] limits = { xmin, ymin, xmax, ymax };

            List<Point3D> plane = pc.getKDTree().getAllPointsAt(depth, 0.05, limits);//0.05 might need changing
            if (plane.Count != 0)
            {
                plane = PointSorter.rotSort(plane);
                plane.Add(plane[0]); //a list eating its own head, steve matthews would be proud

                double circum = 0;

                for (int j = 0; j < plane.Count - 1; j++)
                {
                    circum = circum + Math.Sqrt(Math.Pow((plane[j + 1].X - plane[j].X), 2) + Math.Pow((plane[j + 1].Y - plane[j].Y), 2));
                }

                Console.WriteLine("Circum Pre Multi: " + circum);
                circum = UnitConvertor.convertPCM(circum);
                Console.WriteLine("Circum: " + circum);
                return circum;
            }
            else
            {
                Console.WriteLine("FIBRE'S ARE FUSSED TO THE HEAD!!!");
                return -1;
            }
        }
    }
}
