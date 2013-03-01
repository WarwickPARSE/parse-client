using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE
{
    class UnitConvertor
    {
        private const double pctorwtransform = 0.548984131;//to be determined experimentally for 3 dimensional content such as volume.
        
        public static double convertPC3DMeasurement(double PCM)
        {
            double output = -1;
            output = PCM * pctorwtransform;
            return output;

        }

        public static double convertPC1DMeasurement(double PCM)
        {
            double output = -1;
            output = PCM * Math.Pow(pctorwtransform,1/3);//needs to be cube rooted because the constant is for 3 dimensions and convertPC1D is 1d
            return output;
        }

        public static double convertPC2DMeasurement(double PCM)
        {
            double output = -1;
            output = PCM * Math.Pow(pctorwtransform, 2 / 3);//needs to be cube rooted then squared because the constant is for 3 dimensions and convertPC2D is 2d
            return output;
        }

    }
}
