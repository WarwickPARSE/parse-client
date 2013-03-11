using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
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
using System.IO;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Kinect;
using HelixToolkit.Wpf;

using PARSE.ICP.Stitchers;

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
        private List<List<Point3D>> limbCloud;
        private System.Windows.Forms.Timer pcTimer;
        private CloudVisualisation cloudvis;
        private Dictionary<JointType, double[]> jointDepths;
        private RayHitTestResult rayResult;
        private GroupVisualiser gv;
        private Point3D point1;
        private Point3D point2;
        private Model3D model1;
        private Model3D model2;
        private PointCloud pcd;

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

        public ScanLoader()
        {
            InitializeComponent();
            wantKinect = true;
            this.Loaded += new RoutedEventHandler(ScanLoader_Loaded);
            this.hvpcanvas.MouseDown += new MouseButtonEventHandler(hvpcanvas_MouseDown);
            hitState = 0;
        }

        //Event handlers for viewport interaction

        public ScanLoader(List<PointCloud> fcloud)
        {
            InitializeComponent();
            wantKinect = false;


            //hide buttons from form
            cancel_scan.Visibility = Visibility.Collapsed;
            start_scan.Visibility = Visibility.Collapsed;
            this.instructionblock.Visibility = Visibility.Collapsed;

            gv = new GroupVisualiser(fcloud);

            this.Loaded += new RoutedEventHandler(ScanLoader_Loaded);

            //Threading of data context population to speed up model generation.
            System.Diagnostics.Debug.WriteLine("Loading model");
            this.Dispatcher.Invoke((Action)(() =>
            {
                gv.preprocess();
            }));

            //Assigned threaded object result to the data context.
            this.DataContext = gv;
            //gCloud = fcloud;
            this.hvpcanvas.MouseDown += new MouseButtonEventHandler(hvpcanvas_MouseDown);
            System.Diagnostics.Debug.WriteLine("Model loaded");

            hitState = 0;
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
                gv.preprocess();
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
                jointDepths  = enumerateSkeletonDepths(skeleton);

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
                sandra.Speak("Thank you for your time, you have now been captured.");

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

                GroupVisualiser gg = new GroupVisualiser(fincloud);
                gg.preprocess();
                this.DataContext = gg;

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

                translationVector = fineStitcher.refine(model1,model2,point1,point2);

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
         
            //let's just say left arm for now
            if (pcdexisting!=null)
            {
                LimbCalculator.calculateLimbBounds(pcdexisting, jointDepths, "WAIST");
            }
            else
            {
                LimbCalculator.calculateLimbBounds(pcd, jointDepths, "WAIST");
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
