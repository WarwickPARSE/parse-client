using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace PARSE.Tracking
{
    class SkeletonPosition
    {
        //public SkeletonPoint point1 {get; set;}
        //public SkeletonPoint point2 {get; set;}
        //public SkeletonPoint point3 {get; set;}

        //public float anglexy { get; set; }
        //public float anglez { get; set; }

        Skeleton patient;
        double scannerX;
        double scannerY;
        double angleXY;
        double angleZ;

        public SkeletonPosition(Skeleton p, double x, double y, double anglexy, double anglez)
        {
            this.patient = p;
            this.scannerX = x;
            this.scannerY = y;
            this.angleXY = anglexy;
            this.angleZ = anglez;
        }

        public String getBoneName()
        {
            String bone = "default";

            return bone;
        }

        public String getJoint1Name()
        {
            String joint1 = "default";

            return joint1;
        }

        public String getJoint2Name()
        {
            String joint2 = "default";

            return joint2;
        }

        public float getDistanceFromJoint1()
        {
            float dist1 = 0;

            return dist1;
        }

        public float getDistanceFromJoint2()
        {
            float dist2 = 0;

            return dist2;
        }

        public float getBoneLenght()
        {
            float boneLength = 0;

            return boneLength;
        }
    }
}
