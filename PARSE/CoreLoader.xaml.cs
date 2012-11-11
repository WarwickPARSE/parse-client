//System imports
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

//Kinect imports
using Microsoft.Kinect;

namespace PARSE
{
    /// <summary>
    /// Interaction logic for CoreLoader.xaml
    /// </summary>
    public partial class CoreLoader : Window
    {
        //frame sizes
        private int                                     width;
        private int                                     height;

        //Modelling specific definitions
        private ScannerModeller                         modeller;
        private GeometryModel3D                         model;
        private GeometryModel3D[]                       points;

        //point cloud definitions (will change namespace later)
        private bool                                    generatePC;

        //private bool                                    kinectConnected = false;
        //public int[]                                    realDepthCollection;
        //public int                                      realDepth;
        public int                                      x;
        public int                                      y;
        public int                                      s = 4;
        
        //should the kinect be generating point clouds? 
        public bool                                     pc;         

        //New KinectInterpreter Class
        private KinectInterpreter                       kinectInterp;

        public CoreLoader()
        {
            InitializeComponent();

            //init KinectInterpreter
            kinectInterp  = new KinectInterpreter(vpcanvas2);
               
            //do not generate a point cloud until explicitly told to do so 
            this.pc = false;
            this.generatePC = false; 


            //ui initialization
            lblStatus.Content = kinectInterp.kinectStatus;
            
            if (!kinectInterp.kinectReady)    //Disable controls
            {
                btnSensorUp.IsEnabled = false;
                btnSensorDown.IsEnabled = false;
                btnSensorMax.IsEnabled = false;
                btnSensorMin.IsEnabled = false;
                btnFront.IsEnabled = false;
                btnBack.IsEnabled = false;
            }

        }

        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            kinectInterp.SkeletonFrameReady(sender, e);
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
           this.kinectImager.Source = kinectInterp.DepthImageReady(sender, e);

           /*Point Cloud Stuff, needs to be intergated
           imageFrame.CopyPixelDataTo(this.pixelData);

                    //something's broken here...
                    if (pc)
                    {
                        for (int a = 0; a < 480; a += s)
                            for (int b = 0; b < 640; b += s)
                            {
                                temp = ((ushort)this.pixelData[b + a * 640]) >> 3;
                                ((TranslateTransform3D)points[i].Transform).OffsetZ = temp;
                                i++;
                            }                        
                    }

                    i = 0;

                    //generate the point cloud using the z data
                    if (generatePC) 
                    {
                        int size = height * width;
                        for (int ii = 0; ii < height; ii += s) 
                        {
                            for (int jj = 0; jj < width; jj += s) 
                            {
                                temp = ((ushort)this.pixelData[jj + ii * 640]) >> 3;
                                //if(i== 640*480/32)
                                //((TranslateTransform3D)points[i].Transform).OffsetZ = temp;
                                i++;
                            }
                        }
                    }


                }
                else 
                {
                    return;
                }
           */
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

        }


        private void btnFront_Click(object sender, RoutedEventArgs e)
        {
            //do nothing if there is no kinect detected
            //TODO: make sure something has been read in first - this problem is almost certain to never occur 
            /*if (kinectConnected)
            {
                //set the image to the last one that has been read in by the kinect
                di.setData(this.depthFrame32);

                WriteableBitmap a = new WriteableBitmap(
                                    width,
                                    height,
                                    96, // DpiX
                                    96, // DpiY
                                    PixelFormats.Bgr32,
                                    null);

                //di.dumpToImage(miniOutput, width, height);
            }*/
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

        private void btnRobinButton_Click(object sender, RoutedEventArgs e)
        {
            //enable point cloud generation, istantiate point cloud class  
        //    this.generatePC = true; 
        //    this.pointCloud = new ICP.PointCloud(this.width, this.height);

            /*
            pointCloud.setX();
            pointCloud.setY();
            pointCloud.setZ();
            pointCloud.init();
             */
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

                case "Triangle Mesh":

                    kinectInterp.stopStreams(null);

                    GeometryModel3D[] gm        = new GeometryModel3D[640*480];
                    TriangularPointCloud tpc    = new TriangularPointCloud(vpcanvas2, gm);

                    kinectInterp.startDepthMeshStream(gm);
                    tpc.render();

                    break;

                default:

                    break;

            }
        }


    }
}