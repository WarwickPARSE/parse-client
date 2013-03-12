using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE
{
    static class SubSampler
    {
        public static List<Point3D> averageSubSample(List<Point3D> input, int subSampleFactor, int averageFactor)
        {
            return SubSampler.averageSample(SubSampler.subSample(input, subSampleFactor), averageFactor);
        }

        public static List<Point3D> averageSubSample(List<Point3D> input, int factor)
        {
            return SubSampler.averageSample(SubSampler.subSample(input, factor), factor);
        }
        
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
                double aX = 0;
                double aY = 0;
                double aZ = 0;
                
                while ((i < input.Count) && (i % factor != factor - 1))
                {
                    aX = aX + input[i].X;
                    aY = aY + input[i].Y;
                    aZ = aZ + input[i].Z;
                    i++;
                }
                
                aX = aX / factor;
                aY = aY / factor;
                aZ = aZ / factor;

                Point3D average = new Point3D(aX,aY,aZ);
                output.Add(average);
            }
            return output;
        }
    }
}
