using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//Kinect imports
using Microsoft.Kinect;
namespace PARSE.Tracking
{
    class SensorTracker
    {
        // Scan Process
        MeasurementLoader ScanProcessManager;
        bool CaptureOnStill;
        System.Windows.Controls.TextBlock displayText;

        // Tracking & frame processing
        bool tracking = false;                          // Determines the state of the tracker
        private int gapBetweenFrames = 10;       // How many frames to skip between processed frames?
        private volatile int frameCounter = 0;  // frameCounter % gapbetweenframes ==0  -> process frame
        private volatile int frames = 0;
        RGBTracker tracker;                     // Reference to RGBTracker
        bool capturePosition = false;

        // Sensor properties
        public int x = 0;
        public int y = 0;
        private int prevX = 0;
        private int prevY = 0;
        private int dx = 0;
        private int dy = 0;
        public int framesStill = 0;
        public double angleXY = 0;
        public double angleZ = 0;

        // Kinect
        bool kinectConnected;
        KinectSensor kinectSensor;

        // Frames
        private byte[] colorFrame;
        private short[] depthFrame;
        private Skeleton[] skeletonFrame;
        private WriteableBitmap VisualisationOutput;
        private System.Windows.Controls.Image Visualisation;
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        // Skeletons
        private bool skeletonsIdentified = false;
        private bool captureSkeleton = false;
        private byte doctorSkeletonIndex;
        private byte patientSkeletonIndex;
 
        public bool Start()
        {
            System.Diagnostics.Debug.WriteLine("Start!");

            //Only try to use the Kinect sensor if there is one connected
            if (KinectSensor.KinectSensors.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Waiting for frames...");
                lock (this)
                {
                    tracking = true;
                    captureSkeleton = true;
                }
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No Kinect connected!");
                return false;
            }
        }

        public SensorTracker(System.Windows.Controls.Image visualisation, MeasurementLoader manager, bool captureOnStill, System.Windows.Controls.TextBlock textBlock)
        {
            this.ScanProcessManager = manager;
            this.CaptureOnStill = captureOnStill;
            this.displayText = textBlock;

            kinectConnected = (KinectSensor.KinectSensors.Count > 0); // true;

            if (kinectConnected)
            {
                System.Diagnostics.Debug.WriteLine("Initialising tracker...");

                // Prepare visualisation
                Visualisation = visualisation;
                System.Diagnostics.Debug.WriteLine("Visualisation: " + Visualisation.Uid);

                //Initialize sensor
                kinectSensor = KinectSensor.KinectSensors[0];
                kinectSensor.Start();

                //Enable streams
                kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

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

        private void handleNewFrame(object sender, AllFramesReadyEventArgs frames)
        {
            //Console.WriteLine("Frame!");
            bool run;
            bool skel;
            byte[] thisColorFrame;

            lock (this)
            {
                this.frameCounter++;
                run = ((frameCounter % gapBetweenFrames) == 0   &   (tracking = true));
                skel = captureSkeleton;
            }

            // Process the frame?
            if (run == true)
            {
                using (ColorImageFrame CIF = frames.OpenColorImageFrame())
                using (DepthImageFrame DIF = frames.OpenDepthImageFrame())
                using (SkeletonFrame SD = frames.OpenSkeletonFrame())
                {
                    // Get the frames
                    if (CIF != null & DIF != null)
                    {
                        thisColorFrame = new byte[640 * 480 * 4];
                        CIF.CopyPixelDataTo(thisColorFrame);
                        CIF.CopyPixelDataTo(colorFrame);

                        DIF.CopyPixelDataTo(depthFrame);
                        
                        if (skel)
                            try
                            {
                                SD.CopySkeletonDataTo(skeletonFrame);
                            }
                            catch (Exception e)
                            {
                                //shhh
                            }
                    }
                    else
                    {
                        // Frames automatically disposed via using statement
                        return;
                    }

                    // Need to use local copies to maintain volatility of actual variables
                    int tempX = 0;
                    int tempY = 0;
                    double tempAngle = 0;

                    // Get the values
                    byte[] colorFrame2 = (byte[])thisColorFrame.Clone();
                    tracker.ProcessFrame(colorFrame2, out tempX, out tempY, out tempAngle);

                    lock (this)
                    {
                        // Set the position & angle
                        this.prevX = this.x;
                        this.dx = this.prevX - tempX;
                        this.x = tempX;

                        this.prevY = this.y;
                        this.y = tempY;
                        this.dy = this.prevY - tempY;

                        if (this.dx < 10 & this.dy < 10)
                            this.framesStill++;
                        else
                            this.framesStill = 0;

                        this.displayText.Text = (10 - framesStill).ToString();

                        this.angleXY = tempAngle;

                        this.colorFrame = thisColorFrame;

                        if (this.CaptureOnStill)
                            if (x != 0 & y != 0 & this.framesStill > 10)
                            {
                                this.ScanProcessManager.capture(this.x, this.y, this.angleXY, this.angleZ);
                                CapturePosition();
                            }
                    }
                }
            }
            else
            {
                // Ignore the frame & dispose
                using (ColorImageFrame CIF = frames.OpenColorImageFrame())
                using (DepthImageFrame DIF = frames.OpenDepthImageFrame())
                using (SkeletonFrame SD = frames.OpenSkeletonFrame())
                {
                    // But, get the latest colour frame for display, first!
                    if (frames.OpenColorImageFrame() != null)
                        frames.OpenColorImageFrame().CopyPixelDataTo(colorFrame);

                }
                   //disposeFrames(frames);

            }

            // Post-frame work
            FrameReady();
        }

        private void FrameReady()
        {
            // Put the most recent colour frame up
            UpdateVisualisation();

            bool skeletonNeeded = false;
            lock (this)
            {
                skeletonNeeded = captureSkeleton;
            }
            if (skeletonNeeded && !skeletonsIdentified)
                identifySkeletons();

            bool capture = false;
            lock (this)
            {
                capture = capturePosition;
            }
            if (capture)
                CapturePosition();
                
        }

        /*
         * Take the x, y, z, angles, depthframe & skeletonframe and convert to a skeleton-relative position.
         */
        private void CapturePosition()
        {
            
        }

        private void UpdateVisualisation()
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
                HighlightSensor(this.x, this.y);

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
        }

        private void identifySkeletons()
        {
            if (skeletonFrame != null)
            {
                Skeleton[] frame;
                lock (this)
                {
                    frame = skeletonFrame;
                }

                int doctorIndex;
                float distance = float.MaxValue;
                for (int person = 0; person < frame.GetLength(0); person++)
                {

                    float left_hand_X = frame[person].Joints[JointType.HandLeft].Position.X;
                    float left_hand_Y = frame[person].Joints[JointType.HandLeft].Position.Y;

                    float left_hand_distance = (float) Math.Pow( (Math.Pow(((double)(left_hand_X - x)), 2) + Math.Pow(((double)(left_hand_Y - y)), 2)) , 0.5);
                    if (left_hand_distance < distance)
                    {
                        distance = left_hand_distance;
                        doctorIndex = person;
                    }
                }


                // Finally, reset the variables
                lock (this)
                {
                    captureSkeleton = false;
                    skeletonFrame = null;
                }
            }
            else
                return;
        }

        public void Stop()
        {
            lock (this)
            {
                tracking = false;
                kinectSensor.Stop();
            }
        }

        private void HighlightSensor(int posX, int posY)
        {
            // Get row pointers
            int[] rowHeaders = new int[480];
            for (int row = 0; row < 480; row++)
            {
                rowHeaders[row] = row * 640 * 4;
            }

            for (int row = posY - 4; row < posY + 4; row++)
                for (int col = posX - 4; col < posX + 4; col++)
                {
                    if (row > 0 & col > 0 & row < 480 & col < 640)
                    {
                        int index = rowHeaders[row] + col * 4;
                        colorFrame[index] = 0;
                        colorFrame[index + 1] = 255;
                        colorFrame[index + 2] = 0;
                        colorFrame[index + 3] = 0;
                    }
                }
        }

        private void disposeFrames(AllFramesReadyEventArgs frames)
        {
            // Dispose of the frames when finished
            try
            {
                if (frames.OpenColorImageFrame() != null)
                    frames.OpenColorImageFrame().Dispose();
                if (frames.OpenDepthImageFrame() != null)
                    frames.OpenDepthImageFrame().Dispose(); 
                if (frames.OpenSkeletonFrame() != null)
                    frames.OpenSkeletonFrame().Dispose();
            }
            catch (Exception noFrameException) { };            
        }

    }
}
