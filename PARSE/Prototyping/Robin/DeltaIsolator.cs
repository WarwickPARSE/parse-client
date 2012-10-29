using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE
{ 
    class DeltaIsolator
    {
        //I know this is terrible programming practice, but I am just rapidly prototyping, will clean up soon
        private bool d1Set;
        private bool d2Set; 

        private byte[] firstDepthFrame;
        private byte[] secondDepthFrame;
        
        DeltaIsolator() { 
            //do nothing 
        }

        private bool setData() 
        {

            if (!d1Set) {
                return setFirst();
            }
            else if (!d2Set)
            {
                return setSecond();
            }

            //default to failure
            return false; 
        }

        private bool setFirst() 
        {

            //counterfeit atomicity
            return d1Set = true; 
        }

        private bool setSecond() 
        {

            //counterfeit atomicity
            return d2Set = true; 
        }

        private void process() 
        { 
            //do nothing
        }
    }
}
