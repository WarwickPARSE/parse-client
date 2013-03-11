using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE
{
    static class SubSampler
    {
        public static List<Point3D> removeOutliers(List<Point3D> input, double factor)
        {
            double averageDist = CircumferenceCalculator.calculate(input) / input.Count;

            for (int i = 0; i < input.Count-1; i++)
            {
                if (CircumferenceCalculator.distance(input[i], input[i + 1]) > averageDist)
                {
                    input.RemoveAt(i);
                    continue;
                }
            }

            return input;
        }
        
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
                List<Point3D> sample = new List<Point3D>();
                while ((i < input.Count) && (i % factor != factor - 1))
                {
                    sample.Add(input[i]);
                    i++;
                }
                
                double aX = 0;
                double aY = 0;
                double aZ = 0;
                for (int j = 0; j < sample.Count; j++)
                {
                    aX = aX + sample[j].X;
                    aY = aY + sample[j].Y;
                    aZ = aZ + sample[j].Z;
                }
                aX = aX / sample.Count;
                aY = aY / sample.Count;
                aZ = aZ / sample.Count;

                Point3D average = new Point3D(aX,aY,aZ);
                output.Add(average);
            }
            return output;
        }
    }
}
