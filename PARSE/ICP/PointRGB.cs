using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE.ICP
{
    public struct PointRGB
    {
        public Point3D point { get; set; }

        public double r { get; set; }
        public double g { get; set; }
        public double b { get; set; }

        public PointRGB(double x, double y, double z, double r, double g, double b)
        {
            this.point = new Point3D(x, y, z);

            this.r = r;
            this.g = g;
            this.b = b;
        }
    }
}
