using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE
{
    class UnitConvertor
    {
        private const double pctorwtransform = 0.829104109;//to be determined experimentally
        
        public static double convertPC3DMeasurement(double PCM)
        {
            double output = -1;
            output = PCM * Math.Pow(pctorwtransform, 3);
            return output;

        }

        public static double convertPC1DMeasurement(double PCM)
        {
            double output = -1;
            output = PCM * pctorwtransform;
            return output;
        }

        public static double convertPC2DMeasurement(double PCM)
        {
            double output = -1;
            output = PCM * Math.Pow(pctorwtransform, 2);
            return output;
        }

    }
}
