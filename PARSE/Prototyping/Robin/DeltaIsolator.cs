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

        public bool setData(byte[] dFrame) 
        {

            if (!d1Set) {
                return setFirst(dFrame);
            }
            else if (!d2Set)
            {
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

        private void process() 
        { 
            //not implemented yet
        }
       
        /*public void dumpToImage(Image im, int width, int height) { 
            //create a new writable bitmap 
            WriteableBitmap a = new WriteableBitmap(
                        width,
                        height,
                        96, // DpiX
                        96, // DpiY
                        PixelFormats.Bgr32,
                        null);


        }*/

        public byte[] getData() 
        {
            return this.firstDepthFrame;
        }
    }
}
