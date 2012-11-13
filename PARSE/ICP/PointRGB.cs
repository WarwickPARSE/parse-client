using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE.ICP
{
    public struct PointRGB
    {
        public int x, y, z;
        public int r, g, b;

        public PointRGB(int x, int y, int z, int r, int g, int b)
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
