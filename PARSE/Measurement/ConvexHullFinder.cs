using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
//using Emgu.CV;
using System.Drawing;

namespace PARSE
{
    [Obsolete]
    static class ConvexHullFinder
    {
        public static List<Point3D> findHull(List<Point3D> plane3D)
        {
            /*PointF[] planeF = new PointF[plane3D.Count];

            for (int i = 0; i < plane3D.Count; i++)
            {
                planeF[i] = new PointF((float)plane3D[i].X, (float)plane3D[i].Z);
            }

            MemStorage storage = new MemStorage();
            
            planeF = PointCollection.ConvexHull(planeF, storage, Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE).ToArray();

            plane3D = new List<Point3D>();
            for (int i = 0; i < planeF.Length; i++)
            {
                plane3D.Add(new Point3D(planeF[i].X, 0, planeF[i].Y));
            }
            */
            return new List<Point3D>();
        }
    }
}
