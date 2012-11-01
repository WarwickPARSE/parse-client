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
        
        public DeltaIsolator() { 
            //do nothing 
        }

        public bool setData(byte[] dFrame) 
        {

            if (!d1Set) {
                return setFirst(dFrame);
            }
            else if (!d2Set)
            {
                this.process();
                return setSecond(dFrame);
            }

            //default to failure
            return false; 
        }

        private bool setFirst(byte[] dFrame) 
        {
            this.firstDepthFrame = dFrame;
            //counterfeit atomicity
            return d1Set = true; 
        }

        private bool setSecond(byte[] dFrame) 
        {
            this.secondDepthFrame = dFrame;
            //counterfeit atomicity
            return d2Set = true; 
        }

        public byte[] process() 
        {
            byte[] delta = new byte[firstDepthFrame.Length];

            for (int i = 0; i < firstDepthFrame.Length; i++)
            {
                delta[i] = secondDepthFrame[i];
                delta[i] -= firstDepthFrame[i];
            }

            return delta; 
        }
     
        public byte[] getData() 
        {
            return this.firstDepthFrame;
        }
    }
}
