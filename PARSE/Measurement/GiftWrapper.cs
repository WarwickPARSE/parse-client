using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE
{
    static class GiftWrapper
    {
        public static List<Point3D> wrap(List<Point3D> input)
        {
            List<Point3D> output = new List<Point3D>();

            if (input.Count < 3)
            {
                return input;
            }

            Point3D vPointOnHull = input.Where(p => p.X == input.Min(min => min.X)).First();

            Point3D vEndpoint;
            do
            {
                output.Add(vPointOnHull);
                vEndpoint = output[0];

                for (int i = 1; i < input.Count; i++)
                {
                    if ((vPointOnHull == vEndpoint)
                        || (Orientation(vPointOnHull, vEndpoint, input[i]) == -1))
                    {
                        vEndpoint = input[i];
                    }
                }

                vPointOnHull = vEndpoint;
            } while (vEndpoint != hull[0]);

            return output;
        }

        private static int Orientation(Point3D p1, Point3D p2, Point3D p)
        {
            // Determinant
            double Orin = (p2.X - p1.X) * (p.Z - p1.Z) - (p.X - p1.X) * (p2.Z - p1.Z);

            if (Orin > 0)
                return -1; //          (* Orientaion is to the left-hand side  *)
            if (Orin < 0)
                return 1; // (* Orientaion is to the right-hand side *)

            return 0; //  (* Orientaion is neutral aka collinear  *)
        }
    }
}

/*
