using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE
{
    static class SubSampler
    {
        public static List<Point3D> subSample(List<Point3D> input, int factor)
        {
            List<Point3D> output = new List<Point3D>();
            for (int i = 0; i < input.Count; i++)
            {
                if (i % factor == 0)
                {
                    output.Add(input[i]);
                }
            }
            return output;
        }

        public static List<Point3D> averageSample(List<Point3D> input, int factor)
        {
            List<Point3D> output = new List<Point3D>();
            for (int i = 0; i < input.Count; i++)
            {
                if (i % factor == 0)
                {
                    output.Add(input[i]);
                }
            }
            return output;
        }
    }
}
