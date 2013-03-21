using System;

namespace PARSE
{
    class SubRegionOutOfBoundsException : Exception
    {
        public SubRegionOutOfBoundsException()
        {
            Console.WriteLine("Somethings gone all wibbly.");
        }

        public SubRegionOutOfBoundsException(String S)
        {
            Console.WriteLine(S+" has gone all wibbly.");
        }
    }
}
