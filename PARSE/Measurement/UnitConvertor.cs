using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE
{
    class UnitConvertor
    {
        private const double pctorwtransform = 0.809961686;//to be determined experimentally

        public static double convertPCM(double PCM, int dimension)
        {
            double output = -1;
            output = PCM * Math.Pow(pctorwtransform, dimension);
            return output;
        }
    }
}
