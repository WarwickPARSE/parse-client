﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace PARSE
{
    public class SkeletonPosition
    {
        public Skeleton patient { get; set; }

        public Joint joint1 { get; set; }
        public JointType jointType1 { get; set; }
        public String jointName1 { get; set; }
        public double distanceJ1 { get; set; }
        public double offsetXJ1 { get; set; } //if (-) then position of scanner smaller than position of joint
        public double offsetYJ1 { get; set; }
        public double offsetZJ1 { get; set; }

        public double angleXY { get; set; }
        public double angleZ { get; set; }

        public SkeletonPoint point1 { get; set; }
        public SkeletonPoint point2 { get; set; }
        public SkeletonPoint point3 { get; set; }
        ColorImageFormat RGBFrameFormat = ColorImageFormat.RgbResolution640x480Fps30;

        /// <summary>
        /// Constructor with no params because we can give the skeleton & position data later.
        /// </summary>
        public SkeletonPosition()
        {
        }

        /// <summary>
        /// Returns a distance metric, which describes the distance between this SP and the provided SP.
        /// </summary>
        /// <remarks>
        /// Used to operate the timer in SensorTracker
        /// </remarks>
        /// <param name="skeletonPosition"></param>
        /// <returns></returns>
        internal double distanceTo(SkeletonPosition targetPosition)
        {
            double distance = 50;
            System.Diagnostics.Debug.WriteLine("JN1 " + this.jointName1);
            System.Diagnostics.Debug.WriteLine("SPJN1 " + targetPosition.jointName1);

            if (this.jointName1 != targetPosition.jointName1)
                return 100;

            else
            {
                double dx = this.offsetXJ1 - targetPosition.offsetXJ1;
                double dy = this.offsetYJ1 - targetPosition.offsetYJ1;

                distance = Math.Pow(dx, 2) + Math.Pow(dy, 2);
                distance = Math.Pow(distance, 1/2);
            }

            return distance;
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
        /*internal void updatePosition(int x, int y, double angleXY, double angleZ, Skeleton skeleton)
        {
            // Update the position?
        }*/

        internal string getDirections(SkeletonPosition targetPosition)
        {
            String output = "";
            if (this.jointName1 != targetPosition.jointName1)
            {
                output += "Wrong joint: Current=" + this.jointName1 + "  - target the " + targetPosition.jointName1;
            }
            else
            {
                double dx = this.offsetXJ1 - targetPosition.offsetXJ1;
                double dy = this.offsetYJ1 - targetPosition.offsetYJ1;
                double dist = Math.Pow(dx, 2) + Math.Pow(dy, 2);
                dist = Math.Pow(dist, 1/2);

                output += "X: " + Math.Round(dx, 1);
                output += ", Y: " + Math.Round(dy, 1);
                output += " d: " + Math.Round(dist, 2);

                Console.WriteLine("Offsets: " +
                    "this: X: " + this.offsetXJ1
                    + ", Y: " + this.offsetYJ1 +
                    " Target: X: " + targetPosition.offsetXJ1
                    + ", Y: " + targetPosition.offsetYJ1);
            }

            return output;
        }
    }
}
