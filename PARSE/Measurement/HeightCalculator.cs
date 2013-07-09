using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE
{
    static class HeightCalculator
    {
        /// <summary>
        /// Returns the height of a point cloud in real world measurements, aka metres
        /// </summary>
        /// <param name="pc">PointCloud</param>
        /// <returns>double</returns>
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
