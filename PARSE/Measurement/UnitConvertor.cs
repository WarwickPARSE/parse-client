using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE
{
    class UnitConvertor
    {
        private const double pctorwtransform = 0.123195634;//to be determined experimentally
        
        public static double convertPCM(double PCM)
        {
            double output = -1;
            output = PCM * pctorwtransform;
            return output;

        }

    }
}
