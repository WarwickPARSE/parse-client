using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE
{
    static class HeightCalculator
    {
        public static double getHeight(PointCloud pc)
        {
            double ymin = pc.getyMin();
            double ymax = pc.getyMax();
            double height = ymax - ymin;
            height = UnitConvertor.convertPCM(height,1);
            return height;
        }
    }
}
