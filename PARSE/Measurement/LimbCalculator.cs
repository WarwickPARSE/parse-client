using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;
using System.Drawing;

namespace PARSE
{
    public class LimbCalculator
    {

        //provide depth z at a given joint co-ordinate and a pointcloud with that particular information.
        public static double calculate(List<List<Point3D>> planes, int planeNo)
        {
            double circum = 0;

            List<Point3D> plane3D = planes[planeNo];

            System.Diagnostics.Debug.WriteLine(Environment.CurrentDirectory);

            //plane3D = GiftWrapper.wrap(plane3D);
            
            for (int j = 0; j < plane3D.Count - 1; j++)
            {
                circum = circum + Math.Sqrt(Math.Pow((plane3D[j + 1].X - plane3D[j].X), 2) + Math.Pow((plane3D[j + 1].Y - plane3D[j].Y), 2));
            }

            Console.WriteLine("Circum Pre Multi: " + circum);
            circum = UnitConvertor.convertPCM(circum,2);
            Console.WriteLine("Circum: " + circum);
            return circum;

        //TO DO FOR GREG DONT YOU DARE TRY AND AVOID IT YOU FUCKER
            //segmenting the plane into desired areas for circumference
            //sounds hard ;)

        }
    }
}
