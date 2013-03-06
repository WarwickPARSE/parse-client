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
            //bool CaptureOnStill;
        System.Windows.Controls.TextBlock displayText;
        enum Capture_Modes {Capture_On_Still, Capture_At_Position};
        int Capture_Mode;
        bool capture_timer_running;
        int capture_timer_length = 15;
        bool tracking = false;


        // Tracking & frame processing
        private int gapBetweenFrames = 10;       // How many frames to skip between processed frames?
        private volatile int frameCounter = 0;  // frameCounter % gapbetweenframes ==0  -> process frame
        private volatile int frames = 0;
        RGBTracker tracker;                     // Reference to RGBTracker
        //bool capturePosition = false;

        // Sensor properties
        public int x = 0;
        public int y = 0;
        private int prevX = 0;
        private int prevY = 0;
        private int dx = 0;
        private int dy = 0;
        public int captureTimer = 0;
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
        //private bool captureSkeleton = false;
        private byte doctorSkeletonID;
        private byte patientSkeletonID;
        private int activeSkeletons = 0;
 
        private bool start()
        {
            System.Diagnostics.Debug.WriteLine("Start!");

            //Only try to use the Kinect sensor if there is one connected
            if (KinectSensor.KinectSensors.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Waiting for frames...");
                lock (this)
                {
                    tracking = true;
                    //captureSkeleton = true;
                }

                this.displayText.Foreground = Brushes.White;

                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No Kinect connected!");
                return false;
            }
        }

        internal void captureNewLocation()
        {
            this.Capture_Mode = (int)Capture_Modes.Capture_On_Still;
            this.capture_timer_running = true;
            this.start();
        }

        internal void captureAtLocation()
        {
            this.Capture_Mode = (int)Capture_Modes.Capture_At_Position;
            this.capture_timer_running = false;
            this.start();
        }

        private void capture()
        {
            CapturePosition();

            this.ScanProcessManager.capture(this.x, this.y, this.angleXY, this.angleZ);
        }

        public SensorTracker(System.Windows.Controls.Image visualisation, MeasurementLoader manager, bool captureOnStill, System.Windows.Controls.TextBlock textBlock)
        {
            this.ScanProcessManager = manager;
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

                            System.Diagnostics.Debug.WriteLine("There should be " + skeletons + " skeletons");

                        }
                    }
                    else
                    {
                        // Frames automatically disposed via using statement
                        return;
                    }

                    //Console.WriteLine("Do stuff");
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
                        this.angleXY = tempAngle;

                        this.colorFrame = thisColorFrame;
                    }

                    // Post-frame work
                    FrameReady();
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
            }
        }

        private void FrameReady()
        {
            //Console.WriteLine("Frame ready running");

            // Put the most recent colour frame up
            UpdateVisualisation();

            // Update timer
            updateCaptureTimer();

            // Identify skeletons if not done yet
            if (!skeletonsIdentified)
                identifySkeletons();

            // Capture at end of timer
            if (this.captureTimer == capture_timer_length)
                capture();
                
        }

        /*
         * Take the x, y, z, angles, depthframe & skeletonframe and convert to a skeleton-relative position.
         */
        private void CapturePosition()
        {
            Console.WriteLine("Capture position!!!"); 
  

        }

        private void updateCaptureTimer()
        {
            // If mode is new scan
            if (this.Capture_Mode == (int)Capture_Modes.Capture_On_Still)
            {
                int temp_activeSkeletons = 0;
                lock (this)
                {
                    temp_activeSkeletons = activeSkeletons;
                }

                // TODO Probably shouldn't do this here
                temp_activeSkeletons = 0;
                if (skeletonFrame != null)
                    for (int index = 0; index < skeletonFrame.Length; index++)
                        if (skeletonFrame[index].TrackingState == SkeletonTrackingState.Tracked)
                            temp_activeSkeletons++;

                lock (this)
                    activeSkeletons = temp_activeSkeletons;

                // If the scanner hasn't moved too much, and there are still two skeletons in the frame!
                if ( (this.dx < 10 & this.dy < 10 & this.x != 0 & this.y != 0)
                    &&
                    (skeletonFrame != null)  // Need this check before trying to check the frame length! (otherwise get NPE)
                    &&
                     (temp_activeSkeletons == 2)
                    )
                    this.captureTimer++;
                else
                    this.captureTimer = 0;

            }
            else
            {
                // Convert to skeleton-relative position, and find the distance.
                // If distance < threshold, start timer.
                
                if (capture_timer_running)
                {
                    // If distance > threshold, stop timer.
                    // If distance < threshold, increment.
                }
            }

        }

        private void UpdateVisualisation()
        {
            //Console.WriteLine("Updating visualisation!");
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

            // Update text
            if (activeSkeletons < 2)
            {
                this.displayText.Text = "Waiting for doctor and patient";
            }
            else if (activeSkeletons == 2)
            {
                if (!skeletonsIdentified)
                {
                    this.displayText.Text = "Identifying doctor & searching for scanner";
                }
                else
                {
                    if ((capture_timer_length - captureTimer) < 11)
                    {
                        this.displayText.FontSize = Math.Max(4 * captureTimer, this.displayText.FontSize);
                        //this.displayText.Foreground.Opacity = Math.Max(5 * (captureTimer), 20);
                        this.displayText.Text = (capture_timer_length - captureTimer).ToString();
                    }
                    else
                    {
                        this.displayText.FontSize = 16;
                        //this.displayText.Foreground.Opacity = Math.Max(5 * (captureTimer), 20);
                        this.displayText.Text = "Waiting...";
                    }
                }

            }   
        }

        private void identifySkeletons()
        {
            System.Diagnostics.Debug.WriteLine("Identifying skeletons...");

            if (skeletonFrame != null)
            {
                Skeleton[] frame;

                lock (this)
                {
                    frame = skeletonFrame;
                }

                int doctorID = -1;
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

                if (distance < 30 & doctorID != -1)
                {
                    skeletonsIdentified = true;
                    System.Diagnostics.Debug.WriteLine("Doctor identified as skeleton ID " + doctorID);
                }
                else if (doctorID == -1)
                {
                    System.Diagnostics.Debug.WriteLine("Sensor tracker could not identify the doctor");
                    if (x == 0 || y == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("This could be because the sensor has not yet been located");
                    }
                }

                int activeSkeletons = 0;
                for (int index = 0; index < frame.Length; index ++)
                    if (frame[index].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        activeSkeletons++;
                        if (frame[index].TrackingId != doctorID)
                            patientSkeletonID = (byte)frame[index].TrackingId;
                    }

                if (activeSkeletons > 2)
                {
                    System.Diagnostics.Debug.WriteLine("There are too many skeletons");
                    doctorSkeletonID = 8;
                    patientSkeletonID = 8;
                }

            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Frame is null -> No skeletons detected");
                return;
            }
        }

        public void Stop()
        {
            lock (this)
            {
                tracking = false;
                kinectSensor.Stop();
            }
        }

        public void close()
        {
            this.Stop();
            kinectSensor.Stop();
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
    }
}
