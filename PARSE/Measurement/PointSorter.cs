using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE
{
    public static class PointSorter
    {
        /// <summary>
        /// compares two points to see which one is further around the clockface with (0,0,0) as an origin. Returns 1 if it is a, -1 if it is b, 0 if points are equal in position. Ignores y co-ordinate.
        /// </summary>
        /// <param name="a">Point3D</param>
        /// <param name="b">Point3D</param>
        /// <returns>int</returns>
        private static int compareTwoPoints(Point3D a, Point3D b)
        {
            double aTanA, aTanB;
            //  Fetch the atans

            aTanA = Math.Atan2(a.Z, a.X) + Math.PI;
            aTanB = Math.Atan2(b.Z, b.X) + Math.PI;

            //  Determine next point in Clockwise rotation
            if (aTanA < aTanB) return -1;
            else if (aTanA > aTanB) return 1;
            return 0;
        }

        /// <summary>
        /// sorts a list of points into a clockwise sorted list
        /// </summary>
        /// <param name="input">List<Point3D></param>
        /// <returns>List<Point3D></returns>
        public static List<Point3D> clockSort(List<Point3D> input)
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

            Point3D center = new Point3D((xmax + xmin) / 2, 0, (zmax + zmin) / 2);
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
