using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;

namespace PARSE
{
    public class LimbCalculator
    {

        //provide depth z at a given joint co-ordinate and a pointcloud with that particular information.
        public static double calculate(List<List<Point3D>> planes, int planeNo)
        {
            double circum = 0;

            for (int j = 0; j < planes[planeNo].Count - 1; j++)
            {
                circum = circum + Math.Sqrt(Math.Pow((planes[planeNo][j + 1].X - planes[planeNo][j].X), 2) + Math.Pow((planes[planeNo][j + 1].Y - planes[planeNo][j].Y), 2));
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
