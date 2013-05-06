using System;
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
        private int gapBetweenFrames = 9;               // How many frames to skip between processed frames?
        private volatile int frameCounter = 0;          // frameCounter % gapbetweenframes ==0  -> process frame
                                                        // TODO Use this to cancel frame processing if getting behind.
        RGBTracker tracker;                             // Reference to RGBTracker
        Color SensorHighlightColor = Brushes.OrangeRed.Color; // The colour to highlight the sensor with
        int FramesWithoutScanner = 0;                   // For how many frames has there been no sensor found?
        int NoFramesThreshold = 5;                      // For how many frames should we ignore the fact that no sensor has been found, 
                                                        // and continue to display a sensor highlight?

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
        private int[] x_array;
        private int[] y_array;
        private double x_actual_skel = 0;               // current x position in skel coordinates
        private double y_actual_skel = 0;               // current y position in skel coordinates
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

            x_array = new int[3];
            y_array = new int[3];
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

                    //new Thread(() =>
                    tracker.ProcessFrame(colorFrame2, out tempX, out tempY, out tempAngle);
                    //).Start();
                    //Console.WriteLine("Valz: " + tempX + ", " + tempY + ", " + tempAngle);

                    // Perform motion smoothing in a function rather than having it all here
                    UpdateCoordinates(tempX, tempY, tempAngle);

                    lock (this)
                    {
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
                    try
                    {
                        // But, get the latest colour frame for display, first!
                        if (frames.OpenColorImageFrame() != null)
                            frames.OpenColorImageFrame().CopyPixelDataTo(colorFrame);
                    }
                    catch (NullReferenceException Frame_Is_Empty)
                    {
                        // Try to ignore it?
                    }
                }

                updateVisualisation();
            }
        }

        private void UpdateCoordinates(int tempX, int tempY, double tempAngle)
        {
            
            if (tempX == 0 || tempY == 0)
            {
                this.FramesWithoutScanner++;
                if (this.FramesWithoutScanner <= NoFramesThreshold)
                {
                    tempX = this.prevX;
                    tempY = this.prevY;
                }
            }
            else
            {
                this.FramesWithoutScanner = 0;
            }

            this.x_array[2] = this.x_array[1];
            this.x_array[1] = this.x_array[0];
            this.x_array[0] = tempX;

            this.y_array[2] = this.y_array[1];
            this.y_array[1] = this.y_array[0];
            this.y_array[0] = tempY;


            /* Use the median values to smooth the motion */
            /* Particularly important because it can move all over the place! */
            int[] x_array_sorted = (int[])x_array.Clone();
            Array.Sort(x_array_sorted);
            tempX = x_array_sorted[1];

            int[] y_array_sorted = (int[])y_array.Clone();
            Array.Sort(y_array_sorted);
            tempY = y_array_sorted[1];
                    

            lock (this)
            {
                // Set the position & angle
                this.prevX = this.x;
                this.dx = this.prevX - tempX;
                this.x = tempX;

                this.prevY = this.y;
                this.dy = this.prevY - tempY;
                this.y = tempY;

                this.angleXY = tempAngle;

                //double x_pos = Math.Round((2.2 * 2 * Math.Tan(57) * tempX) / 640, 4);
                double x_pos = 0.00425 * (this.x - 320);

                double y_pos = Math.Round((2.2 * 2 * Math.Tan(21.5) * (tempY - 240) * -1) / 480, 4);
                //double y_pos2 = (y_pos * -1) + 1.05;
                double y_pos2 = (y_pos * -1) - 0.15;
                // WAS double y_pos2 = (y_pos * -1) - 0.45;
                this.x_actual_skel = x_pos;
                this.y_actual_skel = y_pos2;

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

            // Update timer
            updateCaptureTimer();

            // Capture at end of timer
            if (this.captureTimer == capture_timer_length)
                capture();

            // Put the most recent colour frame up
            updateVisualisation();
        }

        /// <summary>
        /// Gets the current skeleton-relative position of the sensor. Needed for both operation modes.
        /// </summary>
        private void HandleSkeletons()
        {
            // Identify skeletons if not done yet
            if (!skeletonsIdentified)
                identifySkeletons();

            // Update patient position
            IEnumerable<Skeleton> temp = (skeletonFrame.Where(x => x.TrackingId == this.patientSkeletonID));
            if (temp.Count() == 1)
            {
                CurrentPosition.updatePosition(this.x, this.y, this.angleXY, this.angleZ, temp.First());
            }
        }

        /// <summary>
        /// Call the highlighter, choose what text to display, and convert the byte[] image to a bitmap for display
        /// </summary>
        private void updateVisualisation()
        {           
            bool display = true;
            lock (this)
            {
                if (tracking == true)
                    display = true;
                else
                    display = false;
            }

            //System.Diagnostics.Debug.WriteLine("Display = " + display + "X/Y: " + x + ", " + y);

            if (display)
            {
                // Highlight target position, if in Capture_At_Position mode
                if (this.Capture_Mode == (int)Capture_Modes.Capture_At_Position)
                {
                    highlight(this.CurrentPosition.getXinRGBCoords(), this.CurrentPosition.getYinRGBCoords(), 5, this.TargetHighlightColor);
                }

                // Highlight sensor. Apply after highlighting the target, so it appears on top!
                highlightSensor(this.x, this.y);
            }

            if (this.Capture_Mode == (int)Capture_Modes.Capture_On_Still)
            {

                // Update text!
                // If not enough skeletons, not enough people. Wait for them!
                if (activeSkeletons < 2)
                {
                    this.displayText.Text = "Waiting for doctor and patient";
                }
                // If enough people (exactly)...
                else if (activeSkeletons == 2)
                {
                    // Wait to identify the doctor/patient by finding the scanner
                    if (!skeletonsIdentified)
                    {
                        this.displayText.Text = "Identifying patient & searching for scanner";
                    }
                    // Display timer as all is going so well
                    else
                    {
                        if ((capture_timer_length - captureTimer) < 10)
                        {
                            this.displayText.Text = (capture_timer_length - captureTimer).ToString();
                        }
                        else
                        {
                            this.displayText.Text = "Hold scanner still to capture";
                        }
                    }
                }
            }
            else if (Capture_Mode == (int)Capture_Modes.Capture_At_Position)
            {
                this.displayText.Text = "Distance to target: " + CurrentPosition.distanceTo(PositionTarget).ToString();


            }


            //if (this.capture_timer_running && (this.capture_timer_length - this.captureTimer) < 10)
                //this.displayText.Text += (this.capture_timer_length - this.captureTimer);

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
                    &&
                    skeletonsIdentified == true
                    )
                {
                    this.captureTimer++;
                    int remaining = this.capture_timer_length - this.captureTimer;
                    /*
                    if (remaining < 10)
                        this.displayText.Text = "Capture in: " + remaining;
                    else
                        this.displayText.Text = "Hold sensor still to capture";
                     */
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
                            this.displayText.Text += "Capture in: " + remaining;
                        if (remaining < 0)
                        {
                            remaining = 0;
                            captureTimer--;
                        }
                    }
                    // If distance > threshold, need to display guidance to target position
                    else
                    {
                        this.captureTimer = 0;
                        this.displayText.Text = "Move the sensor closer to the target position";
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

            if (skeletonFrame != null & activeSkeletons == 2 & x != 0 & y != 0)
            {
                Skeleton[] frame;

                lock (this)
                {
                    frame = skeletonFrame;
                }

                int doctorID = -1;
                int patientID = -1;

                // Get the people in the frame
                int personID_1 = -1;
                int personID_2 = -1;
                foreach (Skeleton person in frame)
                {
                    if (person.TrackingState == SkeletonTrackingState.Tracked)
                        if (personID_1 == -1)
                            personID_1 = person.TrackingId;
                        else
                            personID_2 = person.TrackingId;
                }

                double person1_position = frame.Where(Skeleton => Skeleton.TrackingId == personID_1).First().Position.X;
                double person2_position = frame.Where(Skeleton => Skeleton.TrackingId == personID_2).First().Position.X;

                // Operator stands on the left (negative position value)
                if (person1_position < person2_position)
                {
                    doctorID = personID_1;
                    patientID = personID_2;
                }
                else
                {
                    doctorID = personID_2;
                    patientID = personID_1;
                }

                // Check they've both been set
                if ((doctorID + patientID) > 0)
                {
                    doctorSkeletonID = (byte)doctorID;
                    patientSkeletonID = (byte)patientID;
                    skeletonsIdentified = true;
                }

            }
            else
            {
                //this.displayText.Text = "Waiting for doctor and patient - there are not 2 people in the frame";
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
        private Boolean capturePosition()
        {
            Console.WriteLine("Capture position!!!");

            SkeletonPosition skeletonPos = new SkeletonPosition();

            // Update patient position
            IEnumerable<Skeleton> patient = (skeletonFrame.Where(x => x.TrackingId == this.patientSkeletonID));
            Console.WriteLine("Patient ID:  " + this.patientSkeletonID);
            if (patient.Count() == 1)
            {
                skeletonPos.patient = patient.First();
                findPosition(skeletonPos);
                skeletonPos.angleXY = angleXY;
                skeletonPos.angleZ = angleZ;

                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Capture Position: patient count != 1. It's " + patient.Count());
                captureTimer--;

                return false;
            }
        }

        //experiment
        private void findPosition(SkeletonPosition sp)
        {
            SkeletonPoint scannerPos = new SkeletonPoint();

            System.Diagnostics.Debug.WriteLine("Finding position");

            scannerPos.X = (float) (x_actual_skel); // divide by 100??
            scannerPos.Y = (float) (y_actual_skel);
            scannerPos.Z = (float) 2.7542;

            double minDist = 20;
            Joint closestJoint = new Joint();

            foreach (Joint j in sp.patient.Joints)
            {
                double dist = Math.Sqrt(Math.Pow((scannerPos.X - j.Position.X), 2) + Math.Pow((scannerPos.Y - j.Position.Y), 2));
                System.Diagnostics.Debug.WriteLine(j.JointType.ToString() + " distance from sensor:" + dist + " Position: " + j.Position.X + ", " + j.Position.Y);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestJoint = j;
                }
            }

            sp.joint1 = closestJoint;
            sp.jointName1 = closestJoint.JointType.ToString();
            sp.distanceJ1 = minDist;
            sp.offsetXJ1 = scannerPos.X - closestJoint.Position.X;
            sp.offsetYJ1 = scannerPos.Y - closestJoint.Position.Y;
            sp.offsetZJ1 = scannerPos.Z - closestJoint.Position.Z;

            System.Diagnostics.Debug.WriteLine("Joint name: " + sp.jointName1);
            System.Diagnostics.Debug.WriteLine("Distance: " + minDist);
            System.Diagnostics.Debug.WriteLine("Scanner Position: ("+x_actual_skel+","+y_actual_skel+")");
        }

        /// <summary>
        /// Call whatever is supposed to happen on the capture event
        /// </summary>
        private void capture()
        {
            if (capturePosition() == true)
                RaiseCaptureEvent();

            //this.ScanProcessManager.capture(this.x, this.y, this.angleXY, this.angleZ);
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
