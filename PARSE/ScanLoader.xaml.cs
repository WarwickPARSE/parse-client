using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Speech.Synthesis;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Kinect;
using HelixToolkit.Wpf;

namespace PARSE
{
    /// <summary>
    /// Interaction logic for ScanLoader.xaml
    /// </summary>
    public partial class ScanLoader : Window
    {
        //point cloud lists for visualisation
        private List<PointCloud> fincloud;
        private PointCloud gCloud;
        private System.Windows.Forms.Timer pcTimer;
        private CloudVisualisation cloudvis;
        private Dictionary<JointType, double[]> jointDepths;
        private Thread visThread;

        //speech synthesizer instances
        private SpeechSynthesizer ss;
        private int countdown;

        //Kinect instance
        private KinectInterpreter kinectInterp;

        //Captured canvas
        private Canvas tmpCanvas;

        public ScanLoader()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ScanLoader_Loaded);
            this.hvpcanvas.MouseDown += new MouseButtonEventHandler(hvpcanvas_MouseDown);
        }

        //Event handlers for viewport interaction

        public ScanLoader(List<PointCloud> fcloud)
        {
            InitializeComponent();

            //hide buttons from form
            cancel_scan.Visibility = Visibility.Collapsed;
            start_scan.Visibility = Visibility.Collapsed;
            this.instructionblock.Visibility = Visibility.Collapsed;

            this.Loaded += new RoutedEventHandler(ScanLoader_Loaded);
            this.DataContext = new CloudVisualisation(fcloud, false);
            fincloud = fcloud;
            this.hvpcanvas.MouseDown += new MouseButtonEventHandler(hvpcanvas_MouseDown);
        }

        public ScanLoader(PointCloud gcloud)
        {
            InitializeComponent();
            //hide buttons from form
            cancel_scan.Visibility = Visibility.Collapsed;
            start_scan.Visibility = Visibility.Collapsed;
            this.instructionblock.Visibility = Visibility.Collapsed;
            GroupVisualiser gv = new GroupVisualiser(gcloud);

            this.Loaded += new RoutedEventHandler(ScanLoader_Loaded);

            //Threading of data context population to speed up model generation.
            System.Diagnostics.Debug.WriteLine("Loading model");
            this.Dispatcher.Invoke((Action)(() =>
            {
                gv.preprocess();
            }));

            //Assigned threaded object result to the data context.
            this.DataContext = gv;
            gCloud = gcloud;
            this.hvpcanvas.MouseDown += new MouseButtonEventHandler(hvpcanvas_MouseDown);
            System.Diagnostics.Debug.WriteLine("Model loaded");
        }


        private void ScanLoader_Loaded(object Sender, RoutedEventArgs e)
        {
            //place relative to coreloader
            this.Top = this.Owner.Top + 70;
            this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - this.Width -20;

            //start scanning procedure
            kinectInterp = new KinectInterpreter(skeloutline);
            kinectInterp.stopStreams();
          
        }

        private void cancel_scan_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void start_scan_Click(object sender, RoutedEventArgs e)
        {


            if (ss == null)
            {
                //initalize speech sythesizer
                ss = new SpeechSynthesizer();
                ss.Rate = 1;
                ss.Volume = 100;
            }
            
            //init kinect
            if (!this.kinectInterp.IsDepthStreamUpdating)
            {
                this.kinectInterp.startDepthStream();
                this.kinectInterp.kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);
            }

            if (!this.kinectInterp.IsSkelStreamUpdating)
            {
                this.kinectInterp.startSkeletonStream();
                this.kinectInterp.kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            }

            if (!this.kinectInterp.IsDepthStreamUpdating)
            {
                this.kinectInterp.startRGBStream();
                this.kinectInterp.kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);
            }

            kinectInterp.calibrate();

            if (kinectInterp.tooFarForward())
            {
                ss.Speak("Step Backward");
                Console.WriteLine("Step Backward");
            }
            else if (kinectInterp.tooFarBack())
            {
                ss.Speak("Step Forward");
                Console.Write("Step Forward");
            }
            else
            {
                ss.Speak("Your positioning is optimal.");
                Console.WriteLine("Your posiitoning is optimal, have some cake.");
                fincloud = new List<PointCloud>();

                //hide buttons from form
                cancel_scan.Visibility = Visibility.Collapsed;
                start_scan.Visibility = Visibility.Collapsed;

                //create new list of pc's
                fincloud = new List<PointCloud>();

                //start new scanning timer.
                pcTimer = new System.Windows.Forms.Timer();
                pcTimer.Tick += new EventHandler(pcTimer_tick);
                
                ss.Speak("Please face the camera.");
                this.instructionblock.Text = "Please face the camera";

                //Initialize and start timerr
                pcTimer.Interval = 10000;
                countdown = 3;
                pcTimer.Start();
            }
        }

        private void pcTimer_tick(Object sender, EventArgs e)
        {
            if (countdown == 3)
            {
                //enable update of skelL, skelR, skelDepth
                this.kinectInterp.enableUpdateSkelVars();

                //get current skeleton tracking state
                Skeleton skeleton = this.kinectInterp.getSkeletons();
                jointDepths  = enumerateSkeletonDepths(skeleton);

                //PointCloud structure methods
                PointCloud frontCloud = new PointCloud(this.kinectInterp.getRGBTexture(), this.kinectInterp.getDepthArray());
                fincloud.Add(frontCloud);

                //freeze skelL skelDepth and skelR
                this.kinectInterp.disableUpdateSkelVars();

                tmpCanvas = skeloutline;

                ss.Speak("Turn left");
                this.instructionblock.Text = "Please turn left";
                countdown--;
            }
            else if (countdown == 2)
            {

                //PointCloud structure methods
                PointCloud rightCloud = new PointCloud(this.kinectInterp.getRGBTexture(), this.kinectInterp.getDepthArray());
                fincloud.Add(rightCloud);

                ss.Speak("Turn left with your back to the camera");
                this.instructionblock.Text = "Turn left with your back to the camera";
                countdown--;
            }
            else if (countdown == 1)
            {

                //PointCloud structure methods
                PointCloud backCloud = new PointCloud(this.kinectInterp.getRGBTexture(), this.kinectInterp.getDepthArray());
                fincloud.Add(backCloud);

                ss.Speak("Turn left once more");
                this.instructionblock.Text = "Please turn left once more";
                countdown--;
            }
            else if (countdown == 0)
            {

                //PointCloud structure methods
                PointCloud leftCloud = new PointCloud(this.kinectInterp.getRGBTexture(), this.kinectInterp.getDepthArray());
                fincloud.Add(leftCloud);

                //Visualisation instantiation based on int array clouds
                cloudvis = new CloudVisualisation(fincloud, false);
                this.DataContext = cloudvis;

                //stop streams
                kinectInterp.stopStreams();
                skeloutline = tmpCanvas;
                skeloutline.Visibility = Visibility.Hidden;

                //Visualisation instantiation based on KDTree array clouds
                this.instructionblock.Text = "Scanning complete.";
                this.instructionblock.Visibility = Visibility.Collapsed;
                pcTimer.Stop();
            }
        }

        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            kinectInterp.SkeletonFrameReady(sender, e);
        }

        private void ColorImageReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            kinectInterp.ColorImageReady(sender, e);
        }

        private void DepthImageReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            kinectInterp.DepthImageReady(sender, e);
        }

        private void SkeletonImageReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            kinectInterp.SkeletonFrameReady(sender, e);
        }

        void hvpcanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Point location = e.GetPosition(hvpcanvas);
            ModelVisual3D result = GetHitTestResult(location);

            this.instructionblock.Visibility = Visibility.Collapsed;

            if (result == null)
            {
                System.Diagnostics.Debug.WriteLine("No click point bubbled");
                return;
            }

            if (result is ModelVisual3D)
            {
                System.Diagnostics.Debug.WriteLine("You clicked " + location.X + "," + location.Y);
                return;
            }

        }

        public int[] averageDepthArray(int[] depthArray)
        {
            Queue<int[]> averageQueue = new Queue<int[]>();

            System.Diagnostics.Debug.WriteLine("Smoothing depth array");

            averageQueue.Enqueue(depthArray);

            int[] sumDepthArray = new int[depthArray.Length];
            int[] averagedDepthArray = new int[depthArray.Length];
            int Denominator = 0;
            int Count = 1;

            foreach (var item in averageQueue)
            {
                // Process each row in parallel
                Parallel.For(0, 480, depthArrayRowIndex =>
                {

                    for (int depthArrayColumnIndex = 0; depthArrayColumnIndex < 640; depthArrayColumnIndex++)
                    {
                        var index = depthArrayColumnIndex + (depthArrayRowIndex * 480);
                        sumDepthArray[index] += item[index] * Count;
                    }
                });

                Denominator += Count;
                Count++;
            }

            Parallel.For(0, 480, depthArrayRowIndex =>
            {
                // Process each pixel in the row
                for (int depthArrayColumnIndex = 0; depthArrayColumnIndex < 640; depthArrayColumnIndex++)
                {
                    var index = depthArrayColumnIndex + (depthArrayRowIndex * 640);
                    averagedDepthArray[index] = (int)(sumDepthArray[index] / Denominator);
                }
            });

            return averagedDepthArray;

        }

        /*Hit testing methods*/

        ModelVisual3D GetHitTestResult(Point location)
        {
            HitTestResult result = VisualTreeHelper.HitTest(hvpcanvas, location);
            if (result != null && result.VisualHit is ModelVisual3D)
            {
                ModelVisual3D visual = (ModelVisual3D)result.VisualHit;
                return visual;
            }

            return null;
        }

        /*Publicly accessible methods*/

        public Dictionary<JointType, double[]> enumerateSkeletonDepths(Skeleton sk)
        {
            //Store double
            Dictionary<JointType, double[]> jointDepths = new Dictionary<JointType, double[]>();

            //Get depths and x,y locations at joints.
            foreach (Joint j in sk.Joints)
            {
                double[] positions = { j.Position.Z, j.Position.X, j.Position.Y };
                jointDepths.Add(j.JointType, positions);
            }

            return jointDepths;
        }

        /* Getters */
        public List<PointCloud> getPointClouds()
        {
            return fincloud;
        }

        public PointCloud getYourMum()
        {
            return gCloud;
        }


        public Dictionary<JointType, double[]> getJointMeasurements()
        {
            return jointDepths;
        }

        /* Setters */
        public void setLimbCues(bool measureMode)
        {
            if (measureMode)
            {
                this.instructionblock.Visibility = Visibility.Visible;
                skeloutline.Visibility = Visibility.Visible;
                this.instructionblock.Text = "Click on an area of the body";
            }
        }
    }
}
