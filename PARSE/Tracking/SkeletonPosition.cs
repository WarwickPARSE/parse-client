using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace PARSE.Tracking
{
    class SkeletonPosition
    {
        public SkeletonBones bones { get; set; }
        public String boneName { get; set; }

        public SkeletonBones[] horizontal { get; set; }
        public String[] horNames { get; set; }
        public SkeletonBones[] vertical { get; set; }
        public String[] verNames { get; set; }


        //public SkeletonPoint point1 {get; set;}
        //public SkeletonPoint point2 {get; set;}
        //public SkeletonPoint point3 {get; set;}

        //public float anglexy { get; set; }
        //public float anglez { get; set; }

        Skeleton patient;

        int scannerX;
        int scannerY;
        double angleXY;
        double angleZ;

        ColorImageFormat RGBFrameFormat = ColorImageFormat.RgbResolution640x480Fps30;

        public SkeletonPosition(Skeleton p, int x, int y, double anglexy, double anglez)
        {
            this.patient = p;
            this.scannerX = x;
            this.scannerY = y;
            this.angleXY = anglexy;
            this.angleZ = anglez;
        }

        /// <summary>
        /// Constructor with no params because we can give the skeleton & position data later.
        /// </summary>
        public SkeletonPosition()
        {
            horizontal = new SkeletonBones[10] { new SkeletonBones(JointType.ShoulderCenter, JointType.ShoulderLeft), new SkeletonBones(JointType.ShoulderCenter, JointType.ShoulderRight), new SkeletonBones(JointType.Spine, JointType.ShoulderLeft), new SkeletonBones(JointType.Spine, JointType.ShoulderRight), new SkeletonBones(JointType.Spine, JointType.HipLeft), new SkeletonBones(JointType.Spine, JointType.HipRight), new SkeletonBones(JointType.HipCenter, JointType.HipLeft), new SkeletonBones(JointType.HipCenter, JointType.HipRight), new SkeletonBones(JointType.AnkleRight, JointType.FootRight), new SkeletonBones(JointType.AnkleLeft, JointType.FootLeft) };
            horNames = new String[10] { "collarboneLeft", "collarboneRight", "chestLeft", "chestRight", "stomachLeft", "stomachRight", "waistLeft", "waistRight", "footLeft", "footRight" };
            vertical = new SkeletonBones[13] { new SkeletonBones(JointType.Head, JointType.ShoulderCenter), new SkeletonBones(JointType.ShoulderCenter, JointType.Spine), new SkeletonBones(JointType.Spine, JointType.HipCenter), new SkeletonBones(JointType.ShoulderLeft, JointType.ElbowLeft), new SkeletonBones(JointType.ElbowLeft, JointType.WristLeft), new SkeletonBones(JointType.WristLeft, JointType.HandLeft), new SkeletonBones(JointType.ShoulderRight, JointType.ElbowRight), new SkeletonBones(JointType.ElbowRight, JointType.WristRight), new SkeletonBones(JointType.WristRight, JointType.HandRight), new SkeletonBones(JointType.HipRight, JointType.KneeRight), new SkeletonBones(JointType.KneeRight, JointType.AnkleRight), new SkeletonBones(JointType.HipLeft, JointType.KneeLeft), new SkeletonBones(JointType.KneeLeft, JointType.AnkleLeft) };
            verNames = new String[13] { "neck", "spineTop", "spineBottom", "upperarmLeft", "upperarmRight", "forearmLeft", "forearmRight", "handLeft", "handRight", "thighLeft", "thighRight", "calfLeft", "calfRight" };
            

        }

        public String getBoneName()
        {
            String bone = "default";

            return bone;
        }

        public String getJoint1Name()
        {
            String joint1 = "default";

            joint1 = patient.Joints[JointType.ElbowLeft].JointType.ToString();

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

        /// <summary>
        /// Returns a distance metric, which describes the distance between this SP and the provided SP.
        /// </summary>
        /// <remarks>
        /// Used to operate the timer in SensorTracker
        /// </remarks>
        /// <param name="skeletonPosition"></param>
        /// <returns></returns>
        internal double distanceTo(SkeletonPosition skeletonPosition)
        {
            // Return 100 for now.
            return 100;
        }

        
        /// <summary>
        /// Returns the sensor's X-Y position in RGB frame coordinates 
        /// </summary>
        /// <remarks>
        /// Used to provide coordinates for highlighting in SensorTracker
        /// </remarks>
        /// <returns></returns>
        internal int getXinRGBCoords()
        {
            return 100;
        }

        /// <summary>
        /// Returns the sensor's X-Y position in RGB frame coordinates
        /// </summary>
        /// /// <remarks>
        /// Used to provide coordinates for highlighting in SensorTracker
        /// </remarks>
        /// <returns></returns>
        internal int getYinRGBCoords()
        {
            return 100;
        }

        /// <summary>
        /// Gives the SP a skeleton
        /// </summary>
        /// <param name="skeleton"></param>
        internal void setSkeleton(Skeleton skeleton)
        {
            this.patient = skeleton;
        }

        /// <summary>
        /// Takes the latest skeleton and sensor position data, and updates the stored position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="angleXY"></param>
        /// <param name="angleZ"></param>
        /// <param name="skeleton"></param>
        internal void updatePosition(int x, int y, double angleXY, double angleZ, Skeleton skeleton)
        {
            // Update the position?
        }
    }
}
