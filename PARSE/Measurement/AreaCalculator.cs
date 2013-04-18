using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE
{
    static class AreaCalculator
    {
        /// <summary>
        /// returns one area for every plane fed. returns real world areas not point cloud areas
        /// </summary>
        /// <param name="planes">List(List(Point3D))</param>
        /// <returns>List(double)</returns>
        public static List<double> getAllAreas(List<List<Point3D>> planes)
        {
            List<double> output = new List<double>();
            for (int i = 0; i < planes.Count; i++)
            {
                output.Add(UnitConvertor.convertPCM(AreaCalculator.calculateArea(planes[i]),2));
            }
            return output;
        }

        /// <summary>
        /// Returns an area for the given plane
        /// </summary>
        /// <param name="plane">List(Point3D)</param>
        /// <returns>double</returns>
        public static double calculateArea(List<Point3D> plane)
        {
            double area = 0;
            for (int j = 0; j < plane.Count - 1; j++)
            {
               area = area + ((plane[j].X * plane[j + 1].Z) - (plane[j + 1].X * plane[j].Z));
            }
            area = Math.Abs(area / 2);
            return area;
        }
    }
}