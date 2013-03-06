using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE
{
    static class AreaCalculator
    {
        //return REAL WORLD AREAS, NOT POINT CLOUD AREAS
        public static List<double> getAllAreas(List<List<Point3D>> planes)
        {
            List<double> output = new List<double>();
            for (int i = 0; i < planes.Count; i++)
            {
                output.Add(UnitConvertor.convertPC2DMeasurement(AreaCalculator.calculateArea(planes[i])));
            }
            return output;
        }

        //return POINT CLOUD AREAS, NOT REAL WORLD AREAS
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
