﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//Kinect imports
using Microsoft.Kinect;
namespace PARSE.Tracking
{
    class SensorTracker
    {
        #region Globals

        /* Scan Process */

        // Scan Process Parameters
        MeasurementLoader ScanProcessManager;           // The class that called me
        public enum Capture_Modes 
            {Capture_On_Still, Capture_At_Position};    // Available capture modes
        public int Capture_Mode;                        // Determines the capture mode

        // Scan Process - Timer
        bool capture_timer_running;                     // Is the timer running?
        int capture_timer_length = 15;                  // How long should the timer take?
        public int captureTimer = 0;                    // The timer itself
        bool tracking = false;                          // Should the system be tracking?
    
        // Tracking & frame processing
        private int gapBetweenFrames = 8;               // How many frames to skip between processed frames?
        private volatile int frameCounter = 0;          // frameCounter % gapbetweenframes ==0  -> process frame
                                                        // TODO Use this to cancel frame processing if getting behind.
        RGBTracker tracker;                             // Reference to RGBTracker
        Color SensorHighlightColor = Brushes.Aquamarine.Color; // The colour to highlight the sensor with

        // Position matching
        SkeletonPosition PositionTarget = new SkeletonPosition();   // The target position for capture
        SkeletonPosition CurrentPosition = new SkeletonPosition();  // The current skeleton-relative position of the sensor
        double DistanceThreshold = 5;                               // The distance below which the positions may be considered the same
        Color TargetHighlightColor = Brushes.Yellow.Color;          // The colour to highlight the target position with

        // Output
        System.Windows.Controls.TextBlock displayText;              // The text element in the vis window
        private System.Windows.Controls.Image Visualisation;        // Write visualisation stuff into here

        // Event to fire when ready to capture from sensor
        public delegate void CaptureEventHandler(object sender, SkeletonPosition skel); // Defines what data is contained in the event
        public event CaptureEventHandler Capture;                                       // Defines the event


        /* Objects, values and parameters for frame processing */

        // Kinect
        bool kinectConnected;                           // do we have a kinect?
        KinectSensor kinectSensor;                      // the kinect!

        // Frames
        private byte[] colorFrame;                      // Put the color frame data in here
        private short[] depthFrame;                     // Put the depth frame data in here
        private Skeleton[] skeletonFrame;               // Put the skeleton frame data in here
        private WriteableBitmap VisualisationOutput;    // Write the final visualisation to here
        private static readonly int Bgr32BytesPerPixel = 
            (PixelFormats.Bgr32.BitsPerPixel + 7) / 8; // Need for image properties.
        private int[] rowHeaders;                       // 'Pointers' to rows in the image byte array

        // Skeletons
        private bool skeletonsIdentified = false;       // Have the doctor/patient been identified?
        private byte doctorSkeletonID;                  // What is the doctor's skeleton ID?
        private byte patientSkeletonID;                 // What is the patient's skeleton ID?
        private int activeSkeletons = 0;                // How many skeletons are in view?

        // Sensor position
        public int x = 0;                               // current x coordinate of sensor
        public int y = 0;                               // current y coordinate of sensor
        private int prevX = 0;                          // previous x coordinate of sensor
        private int prevY = 0;                          // previous y coordinate of sensor
        private int dx = 0;                             // change in x between frames
        private int dy = 0;                             // change in y between frames
        public double angleXY = 0;                      // the angle of the sensor in the x/y plane, from the RGB feed
        public double angleZ = 0;                       // the angle of the sensor in the z plane

        #endregion 

        #region Startup_Methods
        private bool start()
        {
            // Prepare kinect etc.
            // Need in a separate function so can call any time?
            // TODO Make loading faster
            prepare();

            //Only try to use the Kinect sensor if there is one connected
            if (KinectSensor.KinectSensors.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Tracker ready. Waiting for images from Kinect...");
                lock (this)
                {
                    tracking = true;
                    //captureSkeleton = true;
                }
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No Kinect connected!");
                return false;
            }
        }

        public void stop()
        {
            lock (this)
            {
                tracking = false;
                kinectSensor.Stop();
            }
        }

        public void close()
        {
            this.stop();
            kinectSensor.Stop();
        }

        internal void captureNewLocation()
        {
            this.Capture_Mode = (int)Capture_Modes.Capture_On_Still;
            this.capture_timer_running = true;
            this.start();
        }

        internal void captureAtLocation(SkeletonPosition target)
        {
            this.Capture_Mode = (int)Capture_Modes.Capture_At_Position;
            this.PositionTarget = target;
            this.capture_timer_running = false;
            this.start();
        }

        public SensorTracker(System.Windows.Controls.Image visualisation, MeasurementLoader manager, System.Windows.Controls.TextBlock textBlock)
        {
            System.Diagnostics.Debug.WriteLine("Initialising tracking system...");

            // Store given variables
            this.ScanProcessManager = manager;
            this.displayText = textBlock;
            this.Visualisation = visualisation;

            // Get row pointers, to have as reference rather than calculating every time (used by Highlight)
            rowHeaders = new int[480];
            for (int row = 0; row < 480; row++)
            {
                rowHeaders[row] = row * 640 * 4;
            }
        }

        private void prepare()
        {
            kinectConnected = (KinectSensor.KinectSensors.Count > 0);

            if (kinectConnected)
            {
                //Initialize sensor
                kinectSensor = KinectSensor.KinectSensors[0];
                kinectSensor.Start();

                //Enable streams
                kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                kinectSensor.SkeletonStream.Enable();

                // Set up frame event
                tracking = true;
                kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(handleNewFrame);

                // Initialise tracking unit
                tracker = new RGBTracker();

                // Prepare frame variables
                colorFrame = new byte[640 * 480 * 4];
                depthFrame = new short[640 * 480];

                System.Diagnostics.Debug.WriteLine("Tracker ready");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No kinect device connected!");
            }
        }

        #endregion

        #region FrameProcessing
        /// <summary>
        /// Called whenever the frames are ready.
        /// Processes the incoming frames using the RGBTracker.
        /// When processed, calls frameReady for post-frame work & visualisation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="frames"></param>
        private void handleNewFrame(object sender, AllFramesReadyEventArgs frames)
        {
            //Console.WriteLine("Frame!");
            bool run;
            byte[] thisColorFrame;

            lock (this)
            {
                this.frameCounter++;
                run = ((frameCounter % gapBetweenFrames) == 0  & (tracking = true));
            }

            // Process the frame?
            if (run == true)
            {
                //Console.WriteLine("Running!!!!!");

                using (ColorImageFrame CIF = frames.OpenColorImageFrame())
                using (DepthImageFrame DIF = frames.OpenDepthImageFrame())
                using (SkeletonFrame SF = frames.OpenSkeletonFrame())
                {
                    // Get the frames
                    if (CIF != null & DIF != null)
                    {
                        //Console.WriteLine("Get frames");
                        thisColorFrame = new byte[640 * 480 * 4];
                        CIF.CopyPixelDataTo(thisColorFrame);
                        CIF.CopyPixelDataTo(colorFrame);
                        DIF.CopyPixelDataTo(depthFrame);

                        if (SF != null)
                        {
                            skeletonFrame = new Skeleton[SF.SkeletonArrayLength];
                            SF.CopySkeletonDataTo(skeletonFrame);
                            
                            int skeletons = 0;
                            foreach (var skeleton in skeletonFrame)
                            {
                                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                                    skeletons++;
                            }
                            activeSkeletons = skeletons;
                        }
                    }
                    else
                    {
                        // Frames automatically disposed via using statement
                        return;
                    }

                    // Now the frame data has been captured, process it!
                    
                    // Need to use local copies to maintain volatility of actual variables
                    int tempX = 0;
                    int tempY = 0;
                    double tempAngle = 0;

                    // Get the values
                    byte[] colorFrame2 = (byte[])thisColorFrame.Clone();

                    new Thread(() =>
                        tracker.ProcessFrame(colorFrame2, out tempX, out tempY, out tempAngle)
                    ).Start();

                    System.Diagnostics.Debug.WriteLine("Valz: " + tempX + ", " + tempY + ", " + tempAngle);

                    lock (this)
                    {
                        // Set the position & angle
                        this.prevX = this.x;
                        this.dx = this.prevX - tempX;
                        this.x = tempX;

                        this.prevY = this.y;
                        this.y = tempY;
                        this.dy = this.prevY - tempY;
                        this.angleXY = tempAngle;

                        this.colorFrame = thisColorFrame;
                    }

                    // Post-frame work
                    frameReady();
                }
            }
            else
            {
                //Console.WriteLine("Ignore frame - frames = " + this.frameCounter);

                // Ignore the frame & dispose
                using (ColorImageFrame CIF = frames.OpenColorImageFrame())
                using (DepthImageFrame DIF = frames.OpenDepthImageFrame())
                using (SkeletonFrame SF = frames.OpenSkeletonFrame())
                {
                    // But, get the latest colour frame for display, first!
                    if (frames.OpenColorImageFrame() != null)
                        frames.OpenColorImageFrame().CopyPixelDataTo(colorFrame);

                }

                updateVisualisation();
            }
        }

        /// <summary>
        /// Called at the end of handleNewFrame if frames have been processed, to coordinate all post-processing activity (timers, visualisations, etc.)
        /// </summary>
        private void frameReady()
        {
            //Console.WriteLine("Frame ready running");

            // Ensure skeletons are identified, get the current skeleton-relative position of the sensor
            HandleSkeletons();

            // Put the most recent colour frame up
            updateVisualisation();

            // Update timer
            updateCaptureTimer();

            // Identify skeletons if not done yet
            if (!skeletonsIdentified)
                identifySkeletons();

            // Capture at end of timer
            if (this.captureTimer == capture_timer_length)
                capture();
                
        }

        /// <summary>
        /// Gets the current skeleton-relative position of the sensor. Needed for both operation modes.
        /// </summary>
        private void HandleSkeletons()
        {
             CurrentPosition.updatePosition(this.x, this.y, this.angleXY, this.angleZ, skeletonFrame.Where(x => x.TrackingId == this.patientSkeletonID).First() );
        }

        /// <summary>
        /// Call the highlighter, choose what text to display, and convert the byte[] image to a bitmap for display
        /// </summary>
        private void updateVisualisation()
        {           
            bool display = true;
            lock (this)
            {
                if (tracking)
                    display = true;
                else
                    display = false;
            }

            if (display)
            {
                if (this.Capture_Mode == (int)Capture_Modes.Capture_At_Position)
                {
                    highlight(this.CurrentPosition.getXinRGBCoords(), this.CurrentPosition.getYinRGBCoords(), 5, this.TargetHighlightColor);
                }
                // Apply sensor highlight after highlighting the target, so it appears on top
                highlightSensor(this.x, this.y);
            }


            // Update text!

            // If not enough skeletons, not enough people. Wait for them!
            if (activeSkeletons < 2)
            {
            if (!this.displayText.Text.Contains("Waiting for doctor and patient"))
                this.displayText.Text = "Waiting for doctor and patient";
            }
            // If enough people (exactly)...
            else if (activeSkeletons == 2)
            {
                // Wait to identify the doctor/patient by finding the scanner
                if (!skeletonsIdentified)
                {
                    this.displayText.Text = "Identifying doctor & searching for scanner";
                }
                // Display timer as all is going so well
                else
                {
                    if ((capture_timer_length - captureTimer) < 11)
                    {
                        this.displayText.Text = (capture_timer_length - captureTimer).ToString();
                    }
                    else
                    {
                        this.displayText.Text = "Waiting...";
                    }
                }
            }

            // Output processed image
            if (VisualisationOutput == null)
                VisualisationOutput = new WriteableBitmap(640, 480, 90, 90, PixelFormats.Bgr32, null);

            this.VisualisationOutput.WritePixels(
                new Int32Rect(0, 0, 640, 480),
                colorFrame,
                640 * Bgr32BytesPerPixel,
                0);

            Visualisation.Source = VisualisationOutput;
            
        }

        /// <summary>
        /// Update the timer, which counts down to the capture event
        /// </summary>
        private void updateCaptureTimer()
        {
            // If mode is new scan
            if (this.Capture_Mode == (int)Capture_Modes.Capture_On_Still)
            {
                // Find the number of people in the frame - it needs to be 2 for the process to continue
                int temp_activeSkeletons = 0;
                lock (this)
                {
                    temp_activeSkeletons = activeSkeletons;
                }

                // If the scanner hasn't moved too much, and there are still two skeletons in the frame!
                if ((this.dx < 10 & this.dy < 10 & this.x != 0 & this.y != 0)
                    &&
                     (temp_activeSkeletons == 2)
                    )
                {
                    this.captureTimer++;
                    int remaining = this.capture_timer_length - this.captureTimer;
                    if (remaining < 10)
                        this.displayText.Text = "Capture in: " + remaining;
                    else
                        this.displayText.Text = "Hold sensor still to capture";
                }
                else
                {
                    this.captureTimer = 0;
                }

            }
            else
            {
                // Convert to skeleton-relative position, and find the distance.
                // If distance < threshold, start timer.

                if (capture_timer_running)
                {
                    // If distance > threshold, stop timer.
                    // If distance < threshold, increment.
                    capturePosition();
                    double distance = this.CurrentPosition.distanceTo(this.PositionTarget);

                    // If distance < threshold, start/continue timer.
                    if (distance < DistanceThreshold)
                    {
                        this.captureTimer++;
                        int remaining = this.capture_timer_length - this.captureTimer;
                        if (remaining < 10)
                            this.displayText.Text = "Capture in: " + remaining;
                    }
                    // If distance > threshold, need to display guidance to target position
                    else
                    {
                        this.captureTimer = 0;
                        this.displayText.Text = "Move the sensor closer, silly!";
                        // TODO display instructions for moving the sensor closer?
                    }
                }
            }
        }
        
        /// <summary>
        /// Identify the skeletons in the image as patient & doctor, by finding the sensor
        /// </summary>
        private void identifySkeletons()
        {
            System.Diagnostics.Debug.WriteLine("Identifying skeletons...");

            if (skeletonFrame != null & activeSkeletons == 2)
            {
                Skeleton[] frame;

                lock (this)
                {
                    frame = skeletonFrame;
                }

                int doctorID = -1;
                int patientID = -1;
                float distance = float.MaxValue;
                for (int person = 0; person < frame.Length; person++)
                {
                    if (frame[person].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        float left_hand_X = frame[person].Joints[JointType.HandLeft].Position.X;
                        float left_hand_Y = frame[person].Joints[JointType.HandLeft].Position.Y;

                        float left_hand_distance = (float)Math.Pow((Math.Pow(((double)(left_hand_X - x)), 2) + Math.Pow(((double)(left_hand_Y - y)), 2)), 0.5);
                        if (left_hand_distance < distance)
                        {
                            distance = left_hand_distance;
                            doctorID = frame[person].TrackingId;
                        }

                        float right_hand_X = frame[person].Joints[JointType.HandRight].Position.X;
                        float right_hand_Y = frame[person].Joints[JointType.HandRight].Position.Y;

                        float right_hand_distance = (float)Math.Pow((Math.Pow(((double)(right_hand_X - x)), 2) + Math.Pow(((double)(right_hand_Y - y)), 2)), 0.5);
                        if (right_hand_distance < distance)
                        {
                            distance = right_hand_distance;
                            doctorID = frame[person].TrackingId;
                        }
                    }
                }

                // Found a doctor!
                if (distance < 30 & doctorID != -1)
                {
                    //  Try to find the patient
                    for (int index = 0; index < frame.Length; index++)
                        if (frame[index].TrackingState == SkeletonTrackingState.Tracked)
                        {
                            if (frame[index].TrackingId != doctorID)
                                patientID = (byte)frame[index].TrackingId;
                        }

                    // If we find a patient, set skeletonsIdentified
                    if (patientID != -1)
                    {
                        lock (this)
                        {
                            this.displayText.Text = "Doctor and patient identified!";
                            System.Diagnostics.Debug.WriteLine("Doctor identified as skeleton ID " + doctorID + ", and patient as ID: " + patientID);

                            // Hurrah!
                            skeletonsIdentified = true;

                            // Put the skeleton inside the SkeletonPosition object CurrentPosition, so it can track positions relative to the patient's skeleton
                            for (int index = 0; index < frame.Length; index++)
                                if (frame[index].TrackingState == SkeletonTrackingState.Tracked)
                                {
                                    if (frame[index].TrackingId == patientSkeletonID)
                                        CurrentPosition.setSkeleton(frame[index]);
                                }
                            
                            // Store the skeleton IDs
                            doctorSkeletonID = (byte)doctorID;
                            patientSkeletonID = (byte)patientID;
                        }
                    }
                }
                else if (doctorID == -1)
                {
                    System.Diagnostics.Debug.WriteLine("Sensor tracker could not identify the doctor");
                    if (x == 0 || y == 0)
                    {
                        //this.displayText.Text = "The sensor tracker is looking for the sensor...";
                        System.Diagnostics.Debug.WriteLine("This could be because the sensor has not yet been located");
                    }
                }
                else if (distance >= 30)
                for (int index = 0; index < frame.Length; index ++)
                    if (frame[index].TrackingState == SkeletonTrackingState.Tracked)
                        if (frame[index].TrackingId != doctorID)
                            patientSkeletonID = (byte)frame[index].TrackingId;
                {
                    //this.displayText.Text = "Sensor found, but not close enough to a hand";
                    System.Diagnostics.Debug.WriteLine("Sensor found, but not close enough to a hand");
                }
            }
            else
            {
                this.displayText.Text = "Waiting for doctor and patient - there are not 2 people in the frame";
                System.Diagnostics.Debug.WriteLine("There are not 2 skeletons in the frame.");
                return;
            }
        }

        /// <summary>
        /// Highlight the sensor in the visualisation, if it has been found
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        private void highlightSensor(int posX, int posY)
        {
            // No position? Don't highlight!
            if (posX != 0 & posY != 0)
            {
                // Choose a depth-dependant highlight size
                short depth = depthFrame[640 * posY + posX];
                int size = 3;
                if (depth < 9500)
                    size = 8;
                else if (depth > 30000)
                    size = 3;
                else
                {
                    size = (int)(16 - (4 * (depth / 10000)));
                    size = Math.Max(size, 3);
                    size = Math.Min(size, 10);
                }
                highlight(posX, posY, size, this.SensorHighlightColor);
            }
        }

        /// <summary>
        /// Applies a highlight of given position, size and colour to the image.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        /// <param name="colour"></param>
        private void highlight(int x, int y, int size, Color colour)
        {
            // Place the highlight
            for (int row = y - size; row < y + size; row++)
            {
                for (int col = x - size; col < x + size; col++)
                {
                    if (row > 0 & col > 0 & row < 480 & col < 640)
                    {
                        int index = rowHeaders[row] + col * 4;
                        colorFrame[index] = colour.R;
                        colorFrame[index + 1] = colour.G;
                        colorFrame[index + 2] = colour.B;
                        colorFrame[index + 3] = colour.A;
                    }
                }
            }
        }

        #endregion

        #region Capture 
        /// <summary>
        /// Take the x, y, z, angles, depthframe & skeletonframe and convert to a skeleton-relative position.
        /// </summary>
        private void capturePosition()
        {
            Console.WriteLine("Capture position!!!");

            SkeletonPosition skeletonPos = new SkeletonPosition();

            //findArea
            findArea(skeletonFrame[patientSkeletonID], x, y, angleXY, angleZ, skeletonPos);

            //capture
            //capture(skeletonFrame[patientSkeletonID], x, y, angleXY, angleZ, skeletonPos);
        }

        private void findArea(Skeleton pt, int x, int y, double anglexy, double anglez, SkeletonPosition sp)
        {
            int pos = 0;
            String bone = "no bone";
            SkeletonBones[] allBones = new SkeletonBones[23] { new SkeletonBones(JointType.Head, JointType.ShoulderCenter), new SkeletonBones(JointType.ShoulderCenter, JointType.ShoulderLeft), new SkeletonBones(JointType.ShoulderCenter, JointType.ShoulderRight), new SkeletonBones(JointType.ShoulderCenter, JointType.Spine), new SkeletonBones(JointType.Spine, JointType.ShoulderLeft), new SkeletonBones(JointType.Spine, JointType.ShoulderRight), new SkeletonBones(JointType.Spine, JointType.HipCenter), new SkeletonBones(JointType.Spine, JointType.HipLeft), new SkeletonBones(JointType.Spine, JointType.HipRight), new SkeletonBones(JointType.HipCenter, JointType.HipLeft), new SkeletonBones(JointType.HipCenter, JointType.HipRight), new SkeletonBones(JointType.ShoulderLeft, JointType.ElbowLeft), new SkeletonBones(JointType.ElbowLeft, JointType.WristLeft), new SkeletonBones(JointType.WristLeft, JointType.HandLeft), new SkeletonBones(JointType.ShoulderRight, JointType.ElbowRight), new SkeletonBones(JointType.ElbowRight, JointType.WristRight), new SkeletonBones(JointType.WristRight, JointType.HandRight), new SkeletonBones(JointType.HipRight, JointType.KneeRight), new SkeletonBones(JointType.KneeRight, JointType.AnkleRight), new SkeletonBones(JointType.AnkleRight, JointType.FootRight), new SkeletonBones(JointType.HipLeft, JointType.KneeLeft), new SkeletonBones(JointType.KneeLeft, JointType.AnkleLeft), new SkeletonBones(JointType.AnkleLeft, JointType.FootLeft) };
            String[] boneNames = new String[23] { "neck", "collarboneLeft", "collarboneRight", "spineTop", "chestLeft", "chestRight", "spineBottom", "stomachLeft", "stomachRight", "waistLeft", "waistRight", "upperarmLeft", "forearmLeft", "handLeft", "upperarmRight", "forearmRight", "handRight", "thighRight", "calfRight", "footRight", "thighLeft", "calfLeft", "footLeft" };

            int i = 0;
            while ((i < 23) && (pos == 0))
            {
                JointType j1 = allBones[i].joint1;
                JointType j2 = allBones[i].joint2;

                if (((x < pt.Joints[j1].Position.X) && (x > pt.Joints[j2].Position.X) && (y < pt.Joints[j1].Position.Y) && (y > pt.Joints[j2].Position.Y)) || ((x < pt.Joints[j2].Position.X) && (x > pt.Joints[j1].Position.X) && (y < pt.Joints[j2].Position.Y) && (y > pt.Joints[j1].Position.Y)))
                {
                    pos = i;
                    sp.bones = allBones[i];
                    bone = boneNames[i];
                    sp.boneName = bone;
                }
                i++;
            }

            if (bone == "no bone")
            {
                //if on chest
            }

            //print bone
            System.Diagnostics.Debug.WriteLine(bone);

        }

        private void capture(Skeleton pt, int x, int y, double anglexy, double anglez, SkeletonPosition sp)
        {
            JointType j1 = sp.bones.joint1;
            JointType j2 = sp.bones.joint2;

            double dist1x = x - pt.Joints[j1].Position.X;
            double dist2x = x - pt.Joints[j2].Position.X;

            double dist1y = y - pt.Joints[j1].Position.Y;
            double dist2y = y - pt.Joints[j2].Position.Y;
        }

        /// <summary>
        /// Call whatever is supposed to happen on the capture event
        /// </summary>
        private void capture()
        {
            // TODO remove old code

            // old code
            //capturePosition();
            //this.ScanProcessManager.capture(this.x, this.y, this.angleXY, this.angleZ);

            // new code!
            RaiseCaptureEvent();
        }

        private void RaiseCaptureEvent()
        {
            // Raise a 'Capture' event
            // Should contain the current skeletonposition (clone)
            Capture(this, CurrentPosition);
        }

        #endregion
    }
}
