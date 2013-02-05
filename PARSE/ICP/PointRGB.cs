using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE.ICP
{
    public struct PointRGB
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public double r { get; set; }
        public double g { get; set; }
        public double b { get; set; }

        public PointRGB(double x, double y, double z, double r, double g, double b)
        {
            this.x = x;
            this.y = y;
            this.z = z;

            this.r = r;
            this.g = g;
            this.b = b;
        }
    }
}
