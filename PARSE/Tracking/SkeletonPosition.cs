using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace PARSE.Tracking
{
    class SkeletonPosition
    {
        public SkeletonPoint point1 {get; set;}
        public SkeletonPoint point2 {get; set;}
        public SkeletonPoint point3 {get; set;}

        public float anglexy { get; set; }
        public float anglez { get; set; }

        public SkeletonPosition()
        {
            
        }

    }
}
