using System;

namespace PARSE
{
    class SubRegionOutOfBoundsException : Exception
    {
        public String type { get; private set; }
        
        public SubRegionOutOfBoundsException()
        {
            Console.WriteLine("Somethings gone all wibbly.");
        }

        public SubRegionOutOfBoundsException(String S)
        {
            Console.WriteLine(S+" has gone all wibbly.");
            this.type = S;
        }
    }
}
