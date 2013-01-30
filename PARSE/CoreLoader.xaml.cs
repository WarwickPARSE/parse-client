//System imports
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Windows.Interop;
using System.Threading.Tasks;
using HelixToolkit.Wpf;
using Emgu.CV;
using Emgu.CV.Structure;

//Kinect imports
using Microsoft.Kinect;

namespace PARSE
{
    /// <summary>
    /// Interaction logic for CoreLoader.xaml
    /// </summary>
    public partial class CoreLoader : Window
    {
        //Modelling specific definitions
        private GeometryModel3D                         Model;
        private GeometryModel3D                         BaseModel;

        //New KinectInterpreter Class
        private KinectInterpreter                       kinectInterp;

        //Image recognition specific definitions
        private System.Windows.Forms.Timer              surfTimer;
        private bool                                    capturedModel;
        private BitmapSource                            modelimage;
        private bool                                    capturedObject;
        private BitmapSource                            objectimage;
        private int                                     countdown;
        private long                                    matchtime;

        //point cloud handler thread 
        private System.Windows.Forms.Timer              pcTimer; 
        private PointCloudHandler                       pcHandler;
        private List<int[]>                             dps;

        //speech synthesizer instances
        private SpeechSynthesizer                       ss;

        public CoreLoader()
        {
            //Initialize Component
            InitializeComponent();

            //Initialize KinectInterpreter
            kinectInterp = new KinectInterpreter(vpcanvas2);
  
            //ui initialization
            lblStatus.Content = kinectInterp.kinectStatus;
            ss = new SpeechSynthesizer();
            
            if (!kinectInterp.kinectReady)    //Disable controls
            {
                btnSensorUp.IsEnabled = false;
                btnSensorDown.IsEnabled = false;
                btnSensorMax.IsEnabled = false;
                btnSensorMin.IsEnabled = false;
                btnStartScanning.IsEnabled = false;
                btnStopScanning.IsEnabled = false;
                btnDumpToFile.IsEnabled = false; 
            }

            //Miscellaneous modelling definitions
            Model = new GeometryModel3D();
            BaseModel = new GeometryModel3D();

        }

        /// <summary>
        /// Kinect skeleton polling method
        /// </summary>
        /// <param name="sender">originator of event</param>
        /// <param name="e">event ready identifier</param>

        int count = 0;
        
        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (count == 0)
            {
                kinectInterp.SkeletonFrameReady(sender, e);
                this.label4.Content = kinectInterp.instruction;
                this.label4.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// Kinect color polling method
        /// </summary>
        /// <param name="sender">originator of event</param>
        /// <param name="e">event ready identifier</param>
       private void ColorImageReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            this.kinectImager.Source = kinectInterp.ColorImageReady(sender, e);
        }

        /// <summary>
        /// Kinect Depth Polling Method
        /// </summary>
        /// <param name="sender">originator of event</param>
        /// <param name="e">event ready identifier</param>
       private void DepthImageReady(object sender, DepthImageFrameReadyEventArgs e)
       {
           if (count == 0)
           {
               this.kinectImager.Source = kinectInterp.DepthImageReady(sender, e);
           }
           count++;
           if (count > 4)
           {
               count = 0;
           }
       }

       private void SkeletonImageReady(object sender, SkeletonFrameReadyEventArgs e)
       {
           kinectInterp.SkeletonFrameReady(sender, e);
       }

        /// <summary>
        /// WPF Form Methods
        /// </summary>
        /// <param name="sender">originator of event</param>
        /// <param name="e">event identifier</param>

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //additional interface implementation to follow.
        }

        //TODO: prevent the following two methods from crashing if called in quick succession
        private void btnSensorUp_Click(object sender, RoutedEventArgs e)
        {
                if (this.kinectInterp.kinectSensor.ElevationAngle != this.kinectInterp.kinectSensor.MaxElevationAngle)
                {
                    this.kinectInterp.kinectSensor.ElevationAngle += 5;
                }
        }

        private void btnSensorDown_Click(object sender, RoutedEventArgs e)
        {
            if (this.kinectInterp.kinectSensor.ElevationAngle != this.kinectInterp.kinectSensor.MinElevationAngle) {
                this.kinectInterp.kinectSensor.ElevationAngle-=5;
            }
        }

        private void btnSensorMin_Click(object sender, RoutedEventArgs e)
        {
            this.kinectInterp.kinectSensor.ElevationAngle = this.kinectInterp.kinectSensor.MinElevationAngle;
        }

        private void btnSensorMax_Click(object sender, RoutedEventArgs e)
        {
            this.kinectInterp.kinectSensor.ElevationAngle = this.kinectInterp.kinectSensor.MaxElevationAngle;
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (this.kinectInterp.kinectReady)
            {
                this.kinectInterp.kinectSensor.Stop();
            }

        }

        private void btnVisualise_Click(object sender, RoutedEventArgs e)
        {

            String feedChoice   = feedcb.Text;
            String visualChoice = visualcb.Text;

            //Stop all streams
            kinectInterp.stopStreams(feedChoice);

            //Assign feed to bitmap source
            switch (feedChoice)
            {

                case "RGB":
                    kinectInterp.startRGBStream();
                    this.kinectInterp.kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);
                    break;

                case "RGB + Skeletal":
                    kinectInterp.startRGBStream();
                    kinectInterp.startSkeletonStream();
                    this.kinectInterp.kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);
                    this.kinectInterp.kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
                    break;

                case "Depth":
                    kinectInterp.startDepthStream();
                    this.kinectInterp.kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);
                    break;

                case "Depth + Skeletal":
                    kinectInterp.startDepthStream();
                    kinectInterp.startSkeletonStream();
                    this.kinectInterp.kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);
                    this.kinectInterp.kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
                    break;

                case "Skeletal":
                    kinectInterp.startSkeletonStream();
                    this.kinectInterp.kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
                    break;

            }

            switch (visualChoice)
            {

                case "Real Time Triangle Mesh":

                    kinectInterp.stopStreams(null);

                    GeometryModel3D[] gm        = new GeometryModel3D[640*480];
                    TriangularPointCloud tpc    = new TriangularPointCloud(vpcanvas2, gm);

                    kinectInterp.startDepthMeshStream(gm);
                    this.kinectInterp.kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);
                    tpc.render();

                    break;

                case "Static Point Cloud":

                    //kinectInterp.startDepthLinearStream(new GeometryModel3D());

                    kinectInterp.startDepthStream();
                    this.kinectInterp.kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);

                    kinectInterp.startSkeletonStream();
                    this.kinectInterp.kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);

                    kinectInterp.startRGBStream();
                    this.kinectInterp.kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);

                    btnStartScanning.IsEnabled = true;

                    //initialize kinect event
                    kinectImager.Width = 0;
                    vpcanvas.Width = 0;

                    //make label visible
                    this.label4.Content = "Click start to generate point cloud";
                    this.label4.Visibility = System.Windows.Visibility.Visible;

                    break;

                case "SURF":

                    kinectInterp.startRGBStream();
                    this.kinectInterp.kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);
                    this.label4.Visibility = System.Windows.Visibility.Visible;

                    surfTimer = new System.Windows.Forms.Timer();
                    surfTimer.Tick += new EventHandler(surfTimer_tick);
                    surfTimer.Interval = 1000;
                    countdown = 5;
                    surfTimer.Start();
                    
                    break;

                default:

                    break;

            }
        }

        private void btnStartScanning_Click(object sender, RoutedEventArgs e)
        {
            
            //create list structure
            dps = new List<int[]>();

            //start new scanning timer.
            pcTimer = new System.Windows.Forms.Timer();
            pcTimer.Tick += new EventHandler(pcTimer_tick);

            //initalize speech sythesizer
            ss.Rate = 1;
            ss.Volume = 100;
            ss.Speak("Please face the camera Bitch");

            //Initialize and start timerr
            pcTimer.Interval = 10000;
            countdown = 3;
            pcTimer.Start();
        }

        private void pcTimer_tick(Object sender, EventArgs e) 
        {
            if (countdown == 3)
            {
                //enable update of skelL, skelR, skelDepth
                this.kinectInterp.enableUpdateSkelVars();
                dps.Add(this.kinectInterp.getDepthArray());
                
                //freeze skelL skelDepth and skelR
                this.kinectInterp.disableUpdateSkelVars();
                ss.Speak("Turn left bitch");
                countdown--;
            }
            else if (countdown == 2)
            {
                dps.Add(this.kinectInterp.getDepthArray());
                ss.Speak("Turn left bitch show me your ass");
                countdown--;
            }
            else if (countdown == 1)
            {
                dps.Add(this.kinectInterp.getDepthArray());
                ss.Speak("Turn left bitch");
                countdown--;
            }
            else if (countdown == 0) {
                dps.Add(this.kinectInterp.getDepthArray());
                this.DataContext = new StaticPointCloud(dps);
                pcTimer.Stop();
            }
        }

        private void surfTimer_tick(Object sender, EventArgs e)
        {

            if ((countdown > 0) && (!capturedModel)) {
                countdown--;
                label4.Content = "Present scanner to camera...." + countdown.ToString() + "...";
            } else if ((countdown == 0) && (!capturedModel)) {
                capturedModel = true;
                modelimage = kinectInterp.getRGBTexture();
                kinectInterp.startRGBStream();
                this.kinectInterp.kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);
                countdown = 10;
            } else if ((countdown > 0) && (capturedModel)) {
                countdown--;
                label4.Content = "Place scanner on patient..." + countdown.ToString() + "...";
            } else if ((countdown == 0) && (capturedModel)) {
                capturedObject = true;
                objectimage = kinectInterp.getRGBTexture();
                label4.Content = "Looking for scanner..";
                startSurfing();
            } else if ((capturedModel) && (capturedObject)) {
                surfTimer.Stop();
            }

        }

        private void startSurfing()
        {
            //Convert bitmap source to image
            MemoryStream outStream = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            System.Drawing.Bitmap resultBitmap;
            Image<Bgr, Byte> result;

            //Convert model image
            enc.Frames.Add(BitmapFrame.Create(modelimage));
            enc.Save(outStream);
            System.Drawing.Bitmap modbm = new System.Drawing.Bitmap(outStream);

            Image<Gray, Byte> modelImg = new Image<Gray, Byte>(modbm);

            //Convert object image
            enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(objectimage));
            enc.Save(outStream);
            System.Drawing.Bitmap objbm = new System.Drawing.Bitmap(outStream);

            Image<Gray, Byte> objectImg = new Image<Gray, Byte>(objbm);

            SurfDetector sd = new SurfDetector();
            result = sd.Draw(modelImg, objectImg, out matchtime);
            resultBitmap = result.ToBitmap();
            BitmapSource bs = Imaging.CreateBitmapSourceFromHBitmap(resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            this.kinectImager.Source = bs;
            surfTimer.Stop();
        }

        private void VolumeOption_Click(object sender, RoutedEventArgs e)
        {
            //Static call to volume calculation method, pass point cloud yielded from visualisation result.
            System.Windows.Forms.MessageBox.Show("You are too fat");
        }
    }
}