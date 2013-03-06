using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE
{
    static class AreaCalculator
    {
        public static List<double> getAllAreas(List<List<Point3D>> planes)
        {
            List<double> output = new List<double>();
            for (int i = 0; i < planes.Count; i++)
            {
                List<Point3D> plane = planes[i];
                output.Add(calculateArea(plane));
            }
            return output;
        }
        
        public static double calculateArea(List<Point3D> plane)
        {
            double area = 0;
            for (int j = 0; j < plane.Count - 1; j++)
            {
               area = area + ((plane[j].X * plane[j + 1].Z) - (plane[j + 1].X * plane[j].Z));
            }
            area = Math.Abs(area / 2);
            Console.WriteLine("Area1:" + UnitConvertor.convertPC2DMeasurement(area));
            return area;
        }
    }
}
