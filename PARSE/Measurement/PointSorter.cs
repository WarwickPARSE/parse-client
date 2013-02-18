using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE
{
    public static class PointSorter
    {
        private static int compareTwoPoints(Point3D a, Point3D b)
        {
            double aTanA, aTanB;
            //  Fetch the atans

            aTanA = Math.Atan2(a.Z, a.X);
            aTanB = Math.Atan2(b.Z, b.X);

            //  Determine next point in Clockwise rotation
            if (aTanA < aTanB) return -1;
            else if (aTanA > aTanB) return 1;
            return 0;
        }

        public static List<Point3D> rotSort(List<Point3D> input)
        {
            double xmax = double.MinValue;
            double xmin = double.MaxValue;
            double zmax = double.MinValue;
            double zmin = double.MaxValue;
            for (int i = 0; i < input.Count; i++)
            {
                if (input[i].X > xmax)
                {
                    xmax = input[i].X;
                }
                else if (input[i].X < xmin)
                {
                    xmin = input[i].X;
                }
                if (input[i].Z > zmax)
                {
                    zmax = input[i].Z;
                }
                else if (input[i].Z < zmin)
                {
                    zmin = input[i].Z;
                }
            }

            Point3D center = new Point3D((xmax - xmin) / 2, 0, (zmax - zmin) / 2);

            for (int i = 0; i < input.Count; i++)
            {
                input[i] = new Point3D(input[i].X - center.X, 0, input[i].Z - center.Z);
            }

            input.Sort(compareTwoPoints);

            for (int i = 0; i < input.Count; i++)
            {
                input[i] = new Point3D(input[i].X + center.X, 0, input[i].Z + center.Z);
            }

            return input;

        }
    }
}
