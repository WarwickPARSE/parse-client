using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE
{
    class UnitConvertor
    {
        private const double pctorwtransform = 0.554692801;//to be determined experimentally for 3 dimensional content such as volume.
        
        public static double convertPCVolumeMeasurement(double PCM)
        {
            double output = -1;
            output = PCM * pctorwtransform;
            return output;

        }

        public static double convertPCVHeightMeasurement(double PCM)
        {
            double output = -1;
            output = PCM * Math.Pow(pctorwtransform,1/3);//needs to be cube rooted because the constant is for 3 dimensions and convertPCVHeightMeasurement is 1d
            return output;
        }

    }
}
