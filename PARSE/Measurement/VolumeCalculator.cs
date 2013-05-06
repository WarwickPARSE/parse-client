using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Emgu.CV.CvEnum;
using System.Windows.Media.Media3D;
using PARSE.ICP;
using System.IO;

namespace PARSE
{
    public static class VolumeCalculator
    {
        /// <summary>
        /// calculates volume, only works on an amorphus blob. Returns a double in real world space.
        /// </summary>
        /// <param name="planes">List(List(Point3D))</param>
        /// <param name="increment">double</param>
        /// <returns>double</returns>
        public static double volume1stApprox(List<List<Point3D>> planes, double increment)
        {
            
            double volume = 0;
            for (int i = 0; i < planes.Count; i++)
            {
                List<Point3D> plane = planes[i]; 
                if (plane.Count != 0)
                {
                    plane.Add(plane[0]); //a list eating its own head, steve matthews would be proud

                    double area = 0;

                    area = AreaCalculator.calculateArea(plane);
                    
                    volume = volume + (area * increment);
                }
                else
                {
                    Console.WriteLine("Plane EMPTY!!! BAD THINGS WILL HAPPEN");
                }
            }
            volume = UnitConvertor.convertPCM(volume,3);
            return volume;
        }

        public static double calculateSiri(double bmi, double weight, double height)
        {
            //Uses BMI BF Currently

            return Math.Abs(Math.Round(((((1.20*bmi) + (0.23*22)) - (10.8*1)) - 5.4),4));
        }

        public static double calculateBrozek(double volume, double weight, double height)
        {
            //For black people
            // %fat = (457 / Body Density) – 414.2

            //Body density = mass/volume

            double density = weight / volume;

            return ((457 / density) - 414.2);
        }

        public static double calculateBMI(double height, double weight)
        {
            double heightSq = Math.Pow(height, 2);

            return Math.Round((weight / heightSq),4);
        }

        public static double calculateApproxWeight(double volume)
        {
            //The weight of a person in kg is based on their volume * 1000m^3
            //This is done on the basis that a person's density is roughly
            //equivalent to water at 1000m^3.

            return volume * 1000;

        }
    }
}
