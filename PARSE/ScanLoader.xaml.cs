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

        private delegate void OneArgDelegate(GroupVisualiser gv);

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

        public ScanLoader()
        {
            InitializeComponent();

            //hide buttons from form
            cancel_scan.Visibility = Visibility.Collapsed;
            start_scan.Visibility = Visibility.Collapsed;
            this.instructionblock.Visibility = Visibility.Collapsed;
            this.loadingwidgetcontrol.Visibility = Visibility.Visible;

            this.Show();

            //wantKinect = true; // Nathan changed this
            this.Loaded += new RoutedEventHandler(ScanLoader_Loaded);
            this.hvpcanvas.MouseDown += new MouseButtonEventHandler(hvpcanvas_MouseDown);
            db = new DatabaseEngine();
            hitState = 0;

            
        }

        //Event handlers for viewport interaction

        public ScanLoader(List<PointCloud> fcloud)
        {
            
        }
        public void processCloudList(List<PointCloud> fcloud, LoadingWidget loadingControl)
        {
            wantKinect = false;

            System.Diagnostics.Debug.WriteLine("Number of items in fcloud list: " + fcloud.Count);

            this.gv = new GroupVisualiser(fcloud);
            this.hvpcanvas.Visibility = Visibility.Visible;
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
            this.loadingwidgetcontrol.Visibility = Visibility.Collapsed;
        }

        public ScanLoader(PointCloud gcloud)
        {
            InitializeComponent();
            wantKinect = false;
            //hide buttons from form
            cancel_scan.Visibility = Visibility.Collapsed;
            start_scan.Visibility = Visibility.Collapsed;
            this.instructionblock.Visibility = Visibility.Collapsed;
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
            //place relative to coreloader
            this.Top = this.Owner.Top + 70;
            this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - this.Width -20;

            //start scanning procedure
            kinectInterp = new KinectInterpreter(skeloutline);

            if ((wantKinect) && (!this.kinectInterp.isSkeletonEnabled()))
            {
                this.kinectInterp.startSkeletonStream();
                this.kinectInterp.kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            }
            if (!wantKinect)
            {
                kinectInterp.stopStreams();
            }
          
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

                sandra.Speak("Scan Added.");
                sandra.Speak("You have now been captured. Thank you for your time.");

                //stop streams
                kinectInterp.stopStreams();

                //stitch me
                //instantiate the stitcher 
                BoundingBox stitcher = new BoundingBox();

                //jam points into stitcher
                stitcher.add(fincloud);
                stitcher.stitch();

                pcd = stitcher.getResult();
                fincloud = stitcher.getResultList();

                ((CoreLoader)(this.Owner)).setPC(pcd, fincloud);
                
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

            //TODO: This performs the limb segmentation procedure.

            Point location = e.GetPosition(hvpcanvas);
            BoundingBox fineStitcher = new BoundingBox();
            TranslateTransform3D translationVector = new TranslateTransform3D();

            hitState = 1;

            //do manual alignment step 1
            if (hitState == 1)
            {
                //ModelVisual3D result = GetHitTestResult(location);
                //point1 = rayResult.PointHit;
                //model1 = rayResult.ModelHit;

                //System.Diagnostics.Debug.WriteLine(point1.X + ", " + point1.Y + ", " + point1.Z);

                //.hitStathitState = 2;
            }
            else if (hitState == 2)
            {

            //do manual alignment step 2

                ModelVisual3D result = GetHitTestResult(location);
                point2 = rayResult.PointHit;
                model2 = rayResult.ModelHit;

                System.Diagnostics.Debug.WriteLine(point2.X);

                this.viewertext.Content = "Select corresponding point on 2nd point cloud (pairwise)";

                hitState = 4;
            }
            else if (hitState == 3)
            {

                System.Diagnostics.Debug.WriteLine(location.ToString());
                //perform limb circumference height selection.
                //LimbCalculator.calculate(limbCloud, 1);

            }
        }

        public void determineLimb(PointCloud pcdexisting)
        {
         
            //pull in skeleton measures from a temporary file for corbett.parse for now.

            if (true)
            {
                Dictionary<String, double[]> jointDepths = new Dictionary<String, double[]>();
                StreamReader sr = new StreamReader("SKEL.ptemp");
                String line;

                while ((line=sr.ReadLine())!=null)
                {
                    String[] joint = Regex.Split(line,":");
                    String[] positions = Regex.Split(joint[1], ",");

                    double[] jointPos = { Convert.ToDouble(positions[0]), Convert.ToDouble(positions[1]), Convert.ToDouble(Regex.Split(positions[2],"\n")[0]) };
                    jointDepths.Add(joint[0], jointPos);
                }

                foreach (var item in jointDepths.Keys)
                {
                    System.Diagnostics.Debug.WriteLine(item);
                }

                LimbCalculator.calculateLimbBounds(pcdexisting, jointDepths, "ARM_LEFT");
            }
            else
            {

                //if we have passed an existing pointcloud from coreloader
                if (pcdexisting != null)
                {
                   // LimbCalculator.calculateLimbBounds(pcdexisting, jointDepths, "ARM_LEFT");
                }
                else
                //otherwise if we have passed a new scan from coreloader
                {
                   // LimbCalculator.calculateLimbBounds(pcd, jointDepths, "ARM_LEFT");
                }

            }

            //change colour of point cloud for limb selection mode
            gv.setMaterial();
            this.DataContext = gv;

            hitState = 3;

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
