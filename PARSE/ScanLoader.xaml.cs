using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Text.RegularExpressions;
using HelixToolkit.Wpf;
using Microsoft.Kinect;
using PARSE.ICP.Stitchers;

namespace PARSE
{
    /// <summary>
    /// Interaction logic for ScanLoader.xaml
    /// </summary>
    public partial class ScanLoader : Window
    {
        // Scan Window Operation mode
        public enum OperationModes 
        { 
            ShowExistingCloud, ShowExistingPatient, ShowExistingResults, 
            CaptureNewCloud, CaptureNewPatient 
        };
        
        private int mode;
        private Boolean preventClose = false;

        //point cloud lists for visualisation
        private List<PointCloud>                fincloud;
        private PointCloud                      gCloud;
        private System.Windows.Forms.Timer      pcTimer;
        private Dictionary<JointType, double[]> jointDepths;
        private RayHitTestResult                rayResult;
        private volatile GroupVisualiser gv;
        private Point3D                         point2;
        private Model3D                         model2;
        private PointCloud                      pcd;

        //Coreloader modifiable hit state
        public int hitState;

        //speech synthesizer instances
        private SpeechSynthesizer sandra;
        private int countdown;

        //Kinect instance
        private KinectInterpreter kinectInterp;
        private Boolean wantKinect;

        //Captured canvas
        private Canvas tmpCanvas;

        //Database object
        DatabaseEngine db;

        public ScanLoader() { } //parameterless version

        public ScanLoader( int mode )
        {
            InitializeComponent();

            //Window Mode
            this.mode = mode;
            //Activate mouse down event handler
            this.hvpcanvas.MouseDown += new MouseButtonEventHandler(hvpcanvas_MouseDown);
            //Instantiate new database instance
            db = new DatabaseEngine();
            
            
            if (this.mode == (int)OperationModes.ShowExistingCloud)
            {
                //hide buttons from form
                cancel_scan.Visibility = Visibility.Collapsed;
                start_scan.Visibility = Visibility.Collapsed;
                this.instructionblock.Visibility = Visibility.Collapsed;
                this.loadingwidgetcontrol.Visibility = Visibility.Visible;

                //center appropriately
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                this.Height = 480;
                this.Width = 640;
                
                //instantiate new joint depths dictionary.
                jointDepths = new Dictionary<JointType, double[]>();

                
            }
            else if(this.mode == (int)OperationModes.ShowExistingResults) 
            {
                //hide buttons from form
                cancel_scan.Visibility = Visibility.Collapsed;
                start_scan.Visibility = Visibility.Collapsed;
                this.instructionblock.Visibility = Visibility.Collapsed;
                this.loadingwidgetcontrol.Visibility = Visibility.Visible;

                //center appropriately
                WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                this.Top = 60;
                this.Left = (System.Windows.SystemParameters.PrimaryScreenWidth - this.Width) - 20;
                this.Height = 400;
                this.Width = 610;

                //instantiate new joint depths dictionary.
                jointDepths = new Dictionary<JointType, double[]>();
            }
            else
            {
                cancel_scan.Visibility = Visibility.Visible;
                start_scan.Visibility = Visibility.Visible;
                this.instructionblock.Visibility = Visibility.Visible;
                this.loadingwidgetcontrol.Visibility = Visibility.Collapsed;
            }

            this.Loaded += new RoutedEventHandler(ScanLoader_Loaded);
            this.Show();

        }

        void ScanLoader_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.preventClose)
                e.Cancel = true;
        } 

        public void processCloudList(List<PointCloud> fcloud, LoadingWidget loadingControl)
        {
            preventClose = true;
            wantKinect = false;
            this.mode = 0;

            System.Diagnostics.Debug.WriteLine("Number of items in fcloud list: " + fcloud.Count);

            this.gv = new GroupVisualiser(fcloud);
            loadingwidgetcontrol.UpdateProgressBy(2);
            
            // Run this as separate 'job' on UI thread
            this.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                (Action)(() => { 
                    this.gv = new GroupVisualiser(fcloud);
                }));
            loadingwidgetcontrol.UpdateProgressBy(5);

            // Run this as lower priority 'job' (on UI thread),
            // with hope that UI remains a bit responsive
            this.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Input,
                (Action)(() => { 
                    this.gv.preprocess(loadingwidgetcontrol);
                }));
            loadingwidgetcontrol.UpdateProgressBy(5);
            
            this.hvpcanvas.DataContext = this.gv;
            loadingwidgetcontrol.UpdateProgressBy(1);

            this.hitState = 0;

            // FINALLY, show the controls.
            this.hvpcanvas.Visibility = Visibility.Visible;
            this.loadingwidgetcontrol.Visibility = Visibility.Collapsed;

            preventClose = false;

            /*
             * Scanloaderready();
            if (this.mode == (int)OperationModes.CaptureNewCloud)
            {
                this.start_scan.Visibility = Visibility.Visible;
                this.cancel_scan.Visibility = Visibility.Visible;
            }
            else
            {
                this.start_scan.Visibility = Visibility.Collapsed;
                this.cancel_scan.Visibility = Visibility.Collapsed;
            }
            */
        }

        public ScanLoader(PointCloud gcloud)
        {
            InitializeComponent();

            System.Diagnostics.Debug.WriteLine("* * GLCLOUD SCANLOADER CALLED");

            //wantKinect = false; // nathan changed this

            //hide buttons from form
            gv = new GroupVisualiser(gcloud);

            this.Loaded += new RoutedEventHandler(ScanLoader_Loaded);

            //Threading of data context population to speed up model generation.
            System.Diagnostics.Debug.WriteLine("Loading model");
            this.Dispatcher.Invoke((Action)(() =>
            {
                gv.preprocess(null);
            }));

            //Assigned threaded object result to the data context.
            this.DataContext = gv;
            gCloud = gcloud;
            this.hvpcanvas.MouseDown += new MouseButtonEventHandler(hvpcanvas_MouseDown);
            System.Diagnostics.Debug.WriteLine("Model loaded");

            hitState = 0;
        }

        private void ScanLoader_Loaded(object Sender, RoutedEventArgs e)
        {

            if (this.mode == (int)OperationModes.CaptureNewCloud)
            {
                //start scanning procedure
                kinectInterp = new KinectInterpreter(skeloutline);

                if (!this.kinectInterp.isSkeletonEnabled())
                {
                    this.kinectInterp.startSkeletonStream();
                    this.kinectInterp.kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
                }
            }

            System.Diagnostics.Debug.WriteLine("Scan loader loading complete");
          
        }

        private void cancel_scan_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void start_scan_Click(object sender, RoutedEventArgs e)
        {
            if (sandra == null)
            {
                //initalize speech sythesizer
                sandra = new SpeechSynthesizer();
                sandra.Rate = 1;
                sandra.Volume = 100;
            }
            
            //init kinect

            //start scanning procedure
           // kinectInterp = new KinectInterpreter(skeloutline);


            /*if ((wantKinect) && (!this.kinectInterp.isSkeletonEnabled()))
            {
                this.kinectInterp.startSkeletonStream();
                this.kinectInterp.kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            } */

            if (!this.kinectInterp.isSkeletonEnabled())
            {
                System.Diagnostics.Debug.WriteLine("skel enabled");
                this.kinectInterp.startSkeletonStream();
                this.kinectInterp.kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            } 

          /*  if (!wantKinect)
            {
                System.Diagnostics.Debug.WriteLine("AHH HELP " + kinectInterp.skelDepthPublic);
                kinectInterp.stopStreams();
            } */

            if (!this.kinectInterp.isDepthEnabled())
            {
                this.kinectInterp.startDepthStream();
                this.kinectInterp.kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);
            }

            if (!this.kinectInterp.isColorEnabled())
            {
                this.kinectInterp.startRGBStream();
                this.kinectInterp.kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);
            }

            kinectInterp.calibrate();

            System.Diagnostics.Debug.WriteLine("Depth now: " + kinectInterp.skelDepthPublic);

            if (kinectInterp.tooFarForward())
            {
                sandra.Speak("Step Backward");
                Console.WriteLine("Step Backward");
            }
            else if (kinectInterp.tooFarBack())
            {
                sandra.Speak("Step Forward");
                Console.Write("Step Forward");
            }
            else
            {
                sandra.Speak("Your positioning is optimal.");
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
                
                sandra.Speak("Please face the camera.");
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
                //get current skeleton tracking state
                Skeleton skeleton = this.kinectInterp.getSkeletons();
                jointDepths = enumerateSkeletonDepths(skeleton);

                //PointCloud structure methods
                PointCloud frontCloud = new PointCloud(this.kinectInterp.getRGBTexture(), this.kinectInterp.getDepthArray());
                //frontCloud.deleteFloor();
                fincloud.Add(frontCloud);
                sandra.Speak("Scan Added.");

                //freeze skelL skelDepth and skelR
                this.kinectInterp.kinectSensor.SkeletonStream.Disable();

                tmpCanvas = skeloutline;
                skeloutline = tmpCanvas;
                skeloutline.Visibility = Visibility.Collapsed;

                sandra.Speak("Please turn left.");
                this.instructionblock.Text = "Please turn left";
                countdown--;
            }
            else if (countdown == 2)
            {
                //PointCloud structure methods
                PointCloud rightCloud = new PointCloud(this.kinectInterp.getRGBTexture(), this.kinectInterp.getDepthArray());
                //rightCloud.deleteFloor();
                fincloud.Add(rightCloud);
                sandra.Speak("Scan Added.");
                sandra.Speak("Please turn left with your back to the camera.");
                this.instructionblock.Text = "Turn left with your back to the camera";
                countdown--;
            }
            else if (countdown == 1)
            {

                //PointCloud structure methods
                PointCloud backCloud = new PointCloud(this.kinectInterp.getRGBTexture(), this.kinectInterp.getDepthArray());
                //backCloud.deleteFloor();
                fincloud.Add(backCloud);
                sandra.Speak("Scan Added.");
                sandra.Speak("Please turn left once more.");
                this.instructionblock.Text = "Please turn left once more.";
                countdown--;
            }
            else if (countdown == 0)
            {
                //PointCloud structure methods
                PointCloud leftCloud = new PointCloud(this.kinectInterp.getRGBTexture(), this.kinectInterp.getDepthArray());
                //leftCloud.deleteFloor();
                fincloud.Add(leftCloud);

                this.instructionblock.Text = "You have now been captured. Thank you for your time.";

                sandra.Speak("Scan Added.");
                sandra.Speak("You have now been captured. Thank you for your time.");

                //stop streams
                kinectInterp.stopStreams();

                if(this.Owner is CoreLoader)
                {
                    ((CoreLoader)(this.Owner)).setPC(pcd, fincloud);
                    ((CoreLoader)(this.Owner)).LoadPointCloud();
                }
                else if(this.Owner is OptionLoader)
                {
                    ((CoreLoader)((PatientLoader)((OptionLoader)(this.Owner)).Owner).Owner).setPC(pcd, fincloud);
                    ((CoreLoader)((PatientLoader)((OptionLoader)(this.Owner)).Owner).Owner).LoadPointCloud();
                }

                /*
                double height = Math.Round(HeightCalculator.getHeight(pcd), 3);
                ((CoreLoader)(this.Owner)).windowHistory.heightoutput.Content = height + "m";

                GroupVisualiser gg = new GroupVisualiser(fincloud);
                gg.preprocess(null);
                this.DataContext = gg;

                //Visualisation instantiation based on KDTree array clouds
                this.instructionblock.Text = "Scanning complete.";
                this.instructionblock.Visibility = Visibility.Collapsed;

                ((CoreLoader)(this.Owner)).export1.IsEnabled = true;
                ((CoreLoader)(this.Owner)).export2.IsEnabled = true;
                ((CoreLoader)(this.Owner)).removefloor.IsEnabled = true;
                 */
                pcTimer.Stop();

                //TODO: write all these results to the database; sql insertion clauses.
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

            if (rayResult != null)
            {
                point2 = rayResult.PointHit;
                model2 = rayResult.ModelHit;

                System.Diagnostics.Debug.WriteLine(point2.X + ":" + point2.Y + ":" + point2.Z);
            }

        }

        public List<Tuple<double,double,List<List<Point3D>>>> determineLimb(PointCloud pcdexisting)
        {
         
            //pull in skeleton measures from a temporary file for corbett.parse for now.
            kinectInterp = new KinectInterpreter(skeloutline);
            Dictionary<String, double[]> jointDepthsStr = new Dictionary<String, double[]>();


            //temporary tuple for results
            Tuple<double, double, List<List<Point3D>>> T = new Tuple<double, double, List<List<Point3D>>>(0,0,null);
            //permanent list of tuples for passing back to coreLoader
            List<Tuple<double, double, List<List<Point3D>>>> limbMeasures = new List<Tuple<double,double,List<List<Point3D>>>>();

            //Test if we have a kinect otherwise we cannot use coordinate mapper.
            if (KinectSensor.KinectSensors.Count > 0)
            {
                //test if we have already enumerated joint depths, if so, this has followed a recent scan.

                if (jointDepths.Count == 0)
                {

                    StreamReader sr = new StreamReader("SKEL.ptemp");
                    String line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        String[] joint = Regex.Split(line, ":");
                        String[] positions = Regex.Split(joint[1], ",");

                        double[] jointPos = { Convert.ToDouble(positions[0]), Convert.ToDouble(positions[1]), Convert.ToDouble(Regex.Split(positions[2], "\n")[0]) };

                        //convert to depth co-ordinate space
                        SkeletonPoint sp = new SkeletonPoint();
                        sp.X = (float)Convert.ToDouble(jointPos[1]);
                        sp.Y = (float)Convert.ToDouble(jointPos[2]);
                        sp.Z = (float)Convert.ToDouble(jointPos[0]);

                        CoordinateMapper cm = new CoordinateMapper(kinectInterp.kinectSensor);
                        DepthImagePoint dm = cm.MapSkeletonPointToDepthPoint(sp, DepthImageFormat.Resolution640x480Fps30);

                        //convert x and y co-ords to arbitrary point cloud space
                        Tuple<double, double, double> convertedPoints = LimbCalculator.convertToPCCoords(dm.X, dm.Y, sp.Z);
                        double[] jointPos2 = { convertedPoints.Item3, convertedPoints.Item1, convertedPoints.Item2 };

                        //place back into jointDepths array in terms of depth space.
                        jointDepthsStr.Add(joint[0], jointPos2);
                    }

                }
                else
                {
                    //we have some live skeleton depths, enumerate into strings
                    foreach(JointType j in jointDepths.Keys) {

                        jointDepthsStr = new Dictionary<String, double[]>();
                        jointDepthsStr.Add(j.ToString(),jointDepths[j]);

                    }

                }

                for (int limbArea = 1; limbArea <= 8; limbArea++)
                {
                    //pass point cloud and correct bounds to Limb Calculator
                    //shoulders is first option in list so pass first.
                    limbMeasures.Add(LimbCalculator.calculateLimbBounds(pcdexisting, jointDepthsStr, limbArea));
                }
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(this, "You need a Kinect to perform this action.",
"Kinect Sensor Missing", MessageBoxButton.OK, MessageBoxImage.Stop);
            }

            //change colour of point cloud for limb selection mode
            gv.setMaterial();
            this.DataContext = gv;

            return limbMeasures;

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
            PointHitTestParameters hitParams = new PointHitTestParameters(location);
            VisualTreeHelper.HitTest(hvpcanvas, null, resultCallback, hitParams);

            return null;
        }

        public HitTestResultBehavior resultCallback(HitTestResult result)
        {
            rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                // Did we hit a MeshGeometry3D?
                RayMeshGeometry3DHitTestResult rayMeshResult =
                    rayResult as RayMeshGeometry3DHitTestResult;

            }

            return HitTestResultBehavior.Stop;
        }

        /*Publicly accessible methods*/

        public Dictionary<JointType, double[]> enumerateSkeletonDepths(Skeleton sk)
        {
            //Store double
            jointDepths = new Dictionary<JointType, double[]>();

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
