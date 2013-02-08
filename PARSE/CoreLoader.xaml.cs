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
using System.Xml.Serialization;
using HelixToolkit.Wpf;
using Emgu.CV;
using Emgu.CV.Structure;

//Kinect imports
using Microsoft.Kinect;
using PARSE.Recognition;

namespace PARSE
{
    /// <summary>
    /// Interaction logic for CoreLoader.xaml
    /// </summary>
    public partial class CoreLoader : Window
    {
        //Modelling specific definitions
        private GeometryModel3D Model;
        private GeometryModel3D BaseModel;

        //New KinectInterpreter Class
        private KinectInterpreter kinectInterp;

        //Image recognition specific definitions
        private System.Windows.Forms.Timer surfTimer;
        private bool capturedModel;
        private BitmapSource modelimage;
        private bool capturedObject;
        private BitmapSource objectimage;
        private int countdown;
        private long matchtime;

        //point cloud lists for visualisation
        private List<PointCloud>                        fincloud;
        private System.Windows.Forms.Timer              pcTimer;
        private bool                                    rgbActive;

        //speech synthesizer instances
        private SpeechSynthesizer                       ss;

        public CoreLoader()
        {
            //Initialize Component
            InitializeComponent();

            this.WindowState = WindowState.Maximized;

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

       private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
       {
           WriteableBitmap[] results = new WriteableBitmap[1];
           results = kinectInterp.SensorAllFramesReady(sender, e);
           
           this.kinectImager.Source = results[1];
           this.kinectImager.OpacityMask = new ImageBrush { ImageSource = results[0] };
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
            if (this.kinectInterp.kinectSensor.ElevationAngle != this.kinectInterp.kinectSensor.MinElevationAngle)
            {
                this.kinectInterp.kinectSensor.ElevationAngle -= 5;
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
            kinectInterp.stopStreams();

            //Assign feed to bitmap source
            switch (feedChoice)
            {

                case "RGB":
                    kinectInterp.startRGBStream();
                    this.kinectInterp.kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);
                    break;

                case "RGB + Skeletal":
                    kinectInterp.startRGBStream();
                    kinectInterp.startDepthStream();
                    kinectInterp.startSkeletonStream();
                    this.kinectInterp.kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(SensorAllFramesReady);
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

                case "Multi-SURF":
                    kinectInterp.startRGBStream();
                    this.kinectInterp.kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);

                    multiSurf();
                    //blockSurf();

                    break;

                default:

                    break;

            }
        }

        private void btnStartScanning_Click(object sender, RoutedEventArgs e)
        {

            fincloud = new List<PointCloud>();

            //start new scanning timer.
            pcTimer = new System.Windows.Forms.Timer();
            pcTimer.Tick += new EventHandler(pcTimer_tick);

            //initalize speech sythesizer
            ss.Rate = 1;
            ss.Volume = 100;
            ss.Speak("Please face the camera Bitch");
            this.label4.Content = "Please face the camera";

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

                //PointCloud structure methods
                PointCloud frontCloud = new PointCloud(this.kinectInterp.getRGBTexture(), this.kinectInterp.getDepthArray());
                fincloud.Add(frontCloud);

                //freeze skelL skelDepth and skelR
                this.kinectInterp.disableUpdateSkelVars();
                ss.Speak("Turn left bitch");
                this.label4.Content = "Please turn left";
                countdown--;
            }
            else if (countdown == 2)
            {

                //PointCloud structure methods
                PointCloud rightCloud = new PointCloud(this.kinectInterp.getRGBTexture(), this.kinectInterp.getDepthArray());
                fincloud.Add(rightCloud);

                ss.Speak("Turn left bitch show me your ass");
                this.label4.Content = "Please turn left with your back to the camera";
                countdown--;
            }
            else if (countdown == 1)
            {

                //PointCloud structure methods
                PointCloud backCloud = new PointCloud(this.kinectInterp.getRGBTexture(), this.kinectInterp.getDepthArray());
                fincloud.Add(backCloud);

                ss.Speak("Turn left bitch");
                this.label4.Content = "Please turn left once more";
                countdown--;
            }
            else if (countdown == 0) {

                //PointCloud structure methods
                PointCloud leftCloud = new PointCloud(this.kinectInterp.getRGBTexture(), this.kinectInterp.getDepthArray());
                fincloud.Add(leftCloud);

                //Visualisation instantiation based on int array clouds
                this.DataContext = new CloudVisualisation(fincloud,radioButton1.IsChecked);

                //Visualisation instantiation based on KDTree array clouds

                this.label4.Content = "Scanning complete.";
                pcTimer.Stop();
            }
        }

        private void surfTimer_tick(Object sender, EventArgs e)
        {

            if ((countdown > 0) && (!capturedModel))
            {
                countdown--;
                label4.Content = "Present scanner to camera...." + countdown.ToString() + "...";
            }
            else if ((countdown == 0) && (!capturedModel))
            {
                capturedModel = true;
                modelimage = kinectInterp.getRGBTexture();
                countdown = 10;
            }
            else if ((countdown > 0) && (capturedModel))
            {
                countdown--;
                label4.Content = "Place scanner on patient..." + countdown.ToString() + "...";
            }
            else if ((countdown == 0) && (capturedModel))
            {
                capturedObject = true;
                objectimage = kinectInterp.getRGBTexture();
                label4.Content = "Looking for scanner..";
                startSurfing();
            }
            else if ((capturedModel) && (capturedObject))
            {
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

            System.Diagnostics.Debug.WriteLine("Model Image about to be initialised");
            Image<Gray, Byte> modelImg = new Image<Gray, Byte>(modbm);
            System.Diagnostics.Debug.WriteLine("Model Image initialised");

            //Convert object image
            enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(objectimage));
            enc.Save(outStream);
            System.Drawing.Bitmap objbm = new System.Drawing.Bitmap(outStream);

            Image<Gray, Byte> objectImg = new Image<Gray, Byte>(objbm);

            SurfDetector sd = new SurfDetector();
            result = sd.Draw(modelImg, objectImg, out matchtime).getMappedImage();
            resultBitmap = result.ToBitmap();
            BitmapSource bs = Imaging.CreateBitmapSourceFromHBitmap(resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            this.kinectImager.Source = bs;
            surfTimer.Stop(); 
        }

        private void multiSurf()
        {
            MultiSurf multiSurfController = new MultiSurf();

            /*
            BitmapSource observedImg = kinectInterp.getRGBTexture();
           
            //Convert bitmap source to image
            MemoryStream outStream = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();

            //Convert image
            enc.Frames.Add(BitmapFrame.Create(observedImg));
            enc.Save(outStream);
            System.Drawing.Bitmap observedBitmap = new System.Drawing.Bitmap(outStream);

            Image<Gray, Byte> obsImg = new Image<Gray, Byte>(observedBitmap);

            // TODO Change from .draw to .run or something?

            // Run with image from Kinect
            //Image<Bgr, Byte> result = multiSurfController.run(obsImg);
            */
            // Run with image from file
            Image<Bgr, Byte> result = multiSurfController.run();

            System.Drawing.Bitmap resultBitmap = result.ToBitmap();
            BitmapSource resultBitmapSource = Imaging.CreateBitmapSourceFromHBitmap(resultBitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            this.kinectImager.Source = resultBitmapSource;
        }

        private void blockSurf()
        {
            String Target_Url = "c:/parse/multisurf/camera/1.jpg";

            Console.WriteLine("Opening target image...");
            Image<Bgr, Byte> inputImageRaw;
            try
            {
                //inputImageRaw = new Image<Bgr, Byte>("C:/PARSE/MultiSurf/Specs/Positives15.jpg");
                //inputImageRaw = new Image<Bgr, Byte>("C:/PARSE/Training/1/img/Positives7.jpg");
                inputImageRaw = new Image<Bgr, Byte>(Target_Url);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load target image.");

                if (!System.IO.File.Exists(Target_Url))
                    Console.WriteLine("Target image does not exist!");

                Console.WriteLine(e.InnerException);
                throw e;
            }
            Image<Gray, Byte> Target = inputImageRaw.Convert<Gray, Byte>();
            Console.WriteLine("Opened target image.");


            MultiSurf[] multiSurfController = new MultiSurf[12];
            Image<Bgr, byte>[] multiSurfResults = new Image<Bgr, byte>[12];

            for (int blockX = 0; blockX < 4; blockX++)
            {
                for (int blockY = 0; blockY < 3; blockY++)
                {
                    //Thread.Sleep(2000);

                    int blockNumber = 3 * blockX + blockY;
                    Console.WriteLine("Running on block " + blockNumber + " (" + blockX + ", " + blockY + ")" );

                    Image<Gray, byte> blockData = Target.GetSubRect(new System.Drawing.Rectangle(blockX * 320, blockY * 320, 320, 320));
                    Console.WriteLine("Coords! x1 = " + (blockX * 320) + " x2 = " + (blockX * 320 + 320) + " y1 = " + (blockY * 320) + " y2 = " + (blockY*320 + 320) );

                    multiSurfController[blockNumber] = new MultiSurf();
                    multiSurfResults[blockNumber] = multiSurfController[blockNumber].run(blockData);

                }
            }

            
        }

        private void VolumeOption_Click(object sender, RoutedEventArgs e)
        {
            //Static call to volume calculation method, pass persistent point cloud object
            VolumeCalculator.calculateVolume(fincloud[0]);
            ss.Speak("You are a fat bastard");
            System.Windows.Forms.MessageBox.Show("You are too fat");
        }

        private void LimbOption_Click(object sender, RoutedEventArgs e)
        {
            SkeletonFigure sf = new SkeletonFigure(vpcanvas2);
        }

        private void ImportScan_Click(object sender, RoutedEventArgs e)
        {

            /*Import scan currently imports files based on the assumption that they
             * have been serialized as a visualisation object. Once the point cloud 
             * class has been been implemented, it will assume that it is dealing
             * with point cloud objects which will then be passed to the visualisation
             * method as appropriate */

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".PARSE";
            dlg.Filter = "PARSE Reference Data (.PARSE)|*.PARSE";

            if (dlg.ShowDialog() == true)
            {
                String filename = dlg.FileName;
                this.DataContext = ScanSerializer.deserialize(filename);
                fincloud = ScanSerializer.depthPc;
            }
        }

        private void ExportScan_Click(object sender, RoutedEventArgs e)
        {

            /*ExportScan serializes the visualisation object, once the pointcloud
             * structure has been implemented, it will serialize the pc object
             * rather than this visualisation. Current issue, cant serialize list
             objects that contain arrays apparantely.*/

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".PARSE";
            dlg.Filter = "PARSE Reference Data (.PARSE)|*.PARSE";

            if (dlg.ShowDialog() == true)
            {
                String filename = dlg.FileName;
                ScanSerializer.serialize(filename, fincloud);
            }

        }
       
    }
}