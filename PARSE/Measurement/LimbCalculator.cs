using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;
using Emgu.CV;
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
            PointF[] planeF = new PointF[plane3D.Count];

            for (int i = 0; i < plane3D.Count; i++)
            {
                planeF[i] = new PointF((float) plane3D[i].X,(float) plane3D[i].Z);
            }

            MemStorage storage = new MemStorage();
            planeF = PointCollection.ConvexHull(planeF, storage, Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE).ToArray();
            
            plane3D = new List<Point3D>();
            for (int i = 0; i < planeF.Length; i++)
            {
                plane3D.Add(new Point3D(planeF[i].X,0,planeF[i].Y));
            }

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
