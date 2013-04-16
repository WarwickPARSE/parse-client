using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;
using System.Drawing;

namespace PARSE
{
    public class CircumferenceCalculator
    {

        public static double calculate(List<Point3D> input)
        {
            input = GiftWrapper.wrap(input);

            double circum = 0;

            for (int j = 0; j < input.Count - 1; j++)
            {
                circum = circum + CircumferenceCalculator.distance(input[j], input[j + 1]);
            }

            return circum;
        }

        //provide depth z at a given joint co-ordinate and a pointcloud with that particular information.
        public static double calculate(List<List<Point3D>> planes, int planeNo)
        {

            List<Point3D> plane3D;
            double circum = 0;
            
            //sometimes the bounding can be quite poor depending on skeleton bounding
            //check if any planes have been returned before passing to circum. calc.

            if (planes.Count != 0)
            {

                if (planeNo == 1)
                {
                    plane3D = planes[planeNo - 1];
                }
                else
                {
                    plane3D = planes[planeNo];
                }

                circum = CircumferenceCalculator.calculate(plane3D);

                Console.WriteLine("Circum Pre Multi: " + circum);
                circum = UnitConvertor.convertPCM(circum, 1);
                Console.WriteLine("Circum: " + circum);
            }

            return circum;
        }

        public static double distance(Point3D a, Point3D b)
        {
            return Math.Sqrt(Math.Pow((b.X - a.X), 2) + Math.Pow((b.Y - a.Y), 2)); ;
        }
    }
}
