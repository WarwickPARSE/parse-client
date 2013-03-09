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
        

        //only works on an amorphus blob
        public static double volume1stApprox(List<List<Point3D>> planes, double increment)
        {
            
            double volume = 0;
            for (int i = 0; i < planes.Count; i++)
            {
                List<Point3D> plane = planes[i]; 
                if (plane.Count != 0)
                {
                    plane = PointSorter.rotSort(plane);
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
    }
}
