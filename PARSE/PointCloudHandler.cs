using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;

namespace PARSE
{
    class PointCloudHandler
    {
        private int frequency;          //frequency in milliseconds
        private bool scanning;          //are we actually scanning the point cloud?


        public PointCloudHandler(int frequency) 
        {
            this.frequency = frequency; 
        }

        public void run() 
        {
            while (scanning) 
            {
                //do some point cloud icp jiggery pokery here 

                //done, sleep and wait for the next scan
                Thread.Sleep(frequency);
            }
        }
    }
}
