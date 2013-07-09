using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE
{
    class UnitConvertor
    {
        private const double pctorwtransform = 0.809576175;//determined experimentally

        /// <summary>
        /// converts a point cloud measure into a real world measure of dimensionality dimension. Length is dimension = 1, Area is dimension = 2, Volume is dimension = 3 and so on.
        /// </summary>
        /// <param name="PCM">double</param>
        /// <param name="dimension">int</param>
        /// <returns>double</returns>
        public static double convertPCM(double PCM, int dimension)
        {
            double output = -1;
            output = PCM * Math.Pow(pctorwtransform, dimension);
            return output;
        }
    }
}
