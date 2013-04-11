using System;

namespace PARSE
{
    class SubRegionOutOfBoundsException : Exception
    {
        public String type { get; private set; }
        
        /// <summary>
        /// creates an exception with no additional information
        /// </summary>
        public SubRegionOutOfBoundsException()
        {
            Console.WriteLine("Somethings gone all wibbly.");
        }

        /// <summary>
        /// creates an exception with a type S
        /// </summary>
        /// <param name="S">String</param>
        public SubRegionOutOfBoundsException(String S)
        {
            Console.WriteLine(S+" has gone all wibbly.");
            this.type = S;
        }
    }
}
