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

        //RGB Constants
        private const int                               RedIndex = 2;
        private const int                               GreenIndex = 1;
        private const int                               BlueIndex = 0;

        //Depth point array and frame definitions
        private short[]                                 pixelData;
        private byte[]                                  depthFrame32;
        private WriteableBitmap                         outputBitmap;
        private static readonly int                     Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private DepthImageFormat                        lastImageFormat;

        //RGB point array and frame definitions
        private byte[]                                  colorpixelData;
        private byte[]                                  colorFrameRGB;
        private WriteableBitmap                         outputColorBitmap;
        private ColorImageFormat                        rgbImageFormat;

        //frame sizes
        private int                                     width;
        private int                                     height;

        //Modelling specific definitions
        private ScannerModeller                         modeller;
        private GeometryModel3D                         model;
        private GeometryModel3D[]                       points;

        //point cloud definitions (will change namespace later)
        private ICP.PointCloud                          pointCloud;
        private bool                                    generatePC;

        private bool                                    kinectConnected = false;
        public int[]                                    realDepthCollection;
        public int                                      realDepth;
        public int                                      x;
        public int                                      y;
        public int                                      s = 4;
        
        //should the kinect be generating point clouds? 
        public bool                                     pc;         

        //Kinect sensor
        KinectSensor                                    kinectSensor;

        //Skeleton definitions (Stef)
        private Skeleton[] skeletonData = new Skeleton[6];
        private Dictionary<int, SkeletonFigure> skeletons;


        public CoreLoader()
        {
            InitializeComponent();

            skeletons = new Dictionary<int, SkeletonFigure>();
               
            //do not generate a point cloud until explicitly told to do so 
            this.pc = false;
            this.generatePC = false; 

            //Only try to use the Kinect sensor if there is one connected
            if (KinectSensor.KinectSensors.Count != 0)
            {
                kinectConnected = true;

                //Initialize sensor
                kinectSensor = KinectSensor.KinectSensors[0];

                //Enable streams
                kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                kinectSensor.SkeletonStream.Enable();

                //Start streams
                kinectSensor.Start();

                //Check if streams are ready
                //TODO: there is no justification for isolating these events, it makes life much harder
                kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);
                kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);
                kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonImageReady);

                lblStatus.Content = "Status: Device connected";
            }
            else {
                lblStatus.Content = "Status: No Kinect device detected";

                //Disable controls
                btnSensorUp.IsEnabled = false;
                btnSensorDown.IsEnabled = false;
                btnSensorMax.IsEnabled = false;
                btnSensorMin.IsEnabled = false;
                btnFront.IsEnabled = false;
                btnBack.IsEnabled = false;
            }

        }

        /// <summary>
        /// Kinect color polling method
        /// </summary>
        /// <param name="sender">originator of event</param>
        /// <param name="e">event ready identifier</param>

        private void ColorImageReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    bool colorFormat = this.rgbImageFormat != colorFrame.Format;

                    if (colorFormat)
                    {
                        this.colorpixelData = new byte[colorFrame.PixelDataLength];
                        this.colorFrameRGB = new byte[colorFrame.Width * colorFrame.Height * Bgr32BytesPerPixel];

                        this.outputColorBitmap = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                        this.kinectColorImage.Source = this.outputColorBitmap;
                    }

                    colorFrame.CopyPixelDataTo(this.colorpixelData);

                    this.outputColorBitmap.WritePixels(new Int32Rect(0,0,colorFrame.Width,colorFrame.Height), colorpixelData, colorFrame.Width*Bgr32BytesPerPixel, 0);

                    this.rgbImageFormat = colorFrame.Format;
                }
            }
        }

        /// <summary>
        /// Kinect Depth Polling Method
        /// </summary>
        /// <param name="sender">originator of event</param>
        /// <param name="e">event ready identifier</param>
        /// pc = false added because Bernard Button Slowed it down horribly
        private void DepthImageReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame imageFrame = e.OpenDepthImageFrame())
            {
                if (imageFrame != null)
                {
                    //dirty temporary hack - set global variables 
                    this.height = imageFrame.Height;
                    this.width = imageFrame.Width;

                    bool NewFormat = this.lastImageFormat != imageFrame.Format;
                    int temp = 0;
                    int i = 0;
                    x = imageFrame.Width / 2;
                    y = imageFrame.Height / 2;

                    if (NewFormat)
                    {
                        this.pixelData = new short[imageFrame.PixelDataLength];
                        this.depthFrame32 = new byte[imageFrame.Width * imageFrame.Height * Bgr32BytesPerPixel];

                        this.outputBitmap = new WriteableBitmap(
                        imageFrame.Width,
                        imageFrame.Height,
                        96, // DpiX
                        96, // DpiY
                        PixelFormats.Bgr32,
                        null);
                        this.kinectDepthImage.Source = this.outputBitmap;
                    }

                    imageFrame.CopyPixelDataTo(this.pixelData);

                    byte[] convertedDepthBits = this.ConvertDepthFrame(this.pixelData, ((KinectSensor)sender).DepthStream);

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

                    this.outputBitmap.WritePixels(
                    new Int32Rect(0, 0, imageFrame.Width, imageFrame.Height),
                    convertedDepthBits,
                    imageFrame.Width * Bgr32BytesPerPixel,
                    0);

                    this.lastImageFormat = imageFrame.Format;
                }
                else 
                {
                    return;
                }
            }
        }

        private void SkeletonImageReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletonFrame.CopySkeletonDataTo(skeletonData);

                    // Retrieves Skeleton objects with Tracked state
                    var trackedSkeletons = skeletonData.Where(s => s.TrackingState == SkeletonTrackingState.Tracked);

                    // By default, assume all the drawn skeletons are inactive
                    foreach (SkeletonFigure skeleton in skeletons.Values)
                        skeleton.Status = ActivityState.Inactive;

                    foreach (Skeleton trackedSkeleton in trackedSkeletons)
                    {
                        SkeletonFigure skeletonFigure;
                        // Checks if the tracked skeleton is already drawn.
                        if (!skeletons.TryGetValue(trackedSkeleton.TrackingId, out skeletonFigure))
                        {
                            // If not, create a new drawing on our canvas
                            Canvas.SetTop(vpcanvas2, 0);
                            Canvas.SetLeft(vpcanvas2, 0);
                            skeletonFigure = new SkeletonFigure(vpcanvas2);
                            skeletons.Add(trackedSkeleton.TrackingId, skeletonFigure);
                        }

                        // Update the drawing
                        Update(trackedSkeleton, skeletonFigure);
                        skeletonFigure.Status = ActivityState.Active;
                    }

                    foreach (SkeletonFigure skeleton in skeletons.Values)
                    {
                        // Erase all the still inactive drawings. It means they are not tracked anymore.
                        if (skeleton.Status == ActivityState.Inactive)
                            skeleton.Erase();
                    }
                }
                else
                {
                    return;
                }
            }

        }

        /// <summary>
        /// Updates the specified drawn skeleton with the new positions
        /// </summary>
        /// <param name="skeleton">The skeleton source.</param>
        /// <param name="drawing">The target drawing.</param>
        private void Update(Skeleton skeleton, SkeletonFigure figure)
        {
            foreach (Joint joint in skeleton.Joints)
            {
                // Transforms a SkeletonPoint to a ColorImagePoint
                var colorPoint = kinectSensor.MapSkeletonPointToColor(joint.Position, kinectSensor.ColorStream.Format);
                // Scale the ColorImagePoint position to the current window size
                var point = new Point((int)colorPoint.X / 640.0 * this.ActualWidth, (int)colorPoint.Y / 480.0 * this.ActualHeight);
                // update the position of that joint
                figure.Update(joint.JointType, point);
            }
        }

        /// <summary>
        /// Depth Frame Conversion Method
        /// </summary>
        /// <param name="depthFrame">current depth frame</param>
        /// <param name="depthStream">originating depth stream</param>
        /// <returns>depth pixel data</returns>

        private byte[] ConvertDepthFrame(short[] depthFrame, DepthImageStream depthStream)
        {

            this.realDepthCollection = new int[depthFrame.Length];
            
            int colorPixelIndex = 0;
            for (int i = 0;  i < depthFrame.Length; i++)
            {

                realDepth = depthFrame[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                realDepthCollection[i] = realDepth;

                if (realDepth < 1066)
                {
                    this.depthFrame32[colorPixelIndex++] = (byte)(255 * realDepth / 1066);
                    this.depthFrame32[colorPixelIndex++] = 0;
                    this.depthFrame32[colorPixelIndex++] = 0;
                    ++colorPixelIndex;

                }
                else if ((1066 <= realDepth) && (realDepth < 2133))
                {
                    this.depthFrame32[colorPixelIndex++] = (byte)(255 * (2133 - realDepth) / 1066);
                    this.depthFrame32[colorPixelIndex++] = (byte)(255 * (realDepth - 1066) / 1066);
                    this.depthFrame32[colorPixelIndex++] = 0;
                    ++colorPixelIndex;
                }
                else if ((2133 <= realDepth) && (realDepth < 3199))
                {
                    this.depthFrame32[colorPixelIndex++] = 0;
                    this.depthFrame32[colorPixelIndex++] = (byte)(255 * (3198 - realDepth) / 1066);
                    this.depthFrame32[colorPixelIndex++] = (byte)(255 * (realDepth - 2133) / 1066);
                    ++colorPixelIndex;
                }
                else if (3199 <= realDepth)
                {
                    this.depthFrame32[colorPixelIndex++] = 0;
                    this.depthFrame32[colorPixelIndex++] = 0;
                    this.depthFrame32[colorPixelIndex++] = (byte)(255 * (4000 - realDepth) / 801);
                    ++colorPixelIndex;
                }

            }

            return this.depthFrame32;
        }

        /// <summary>
        /// WPF Form Methods
        /// </summary>
        /// <param name="sender">originator of event</param>
        /// <param name="e">event identifier</param>

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Initialize camera position
            bodycamera.Position = new Point3D(
            0.5,
            0.5,
            bodycamera.Position.Z);

        }


        private void btnFront_Click(object sender, RoutedEventArgs e)
        {
            //do nothing if there is no kinect detected
            //TODO: make sure something has been read in first - this problem is almost certain to never occur 
           /* if (kinectConnected)
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
                if (kinectSensor.ElevationAngle != kinectSensor.MaxElevationAngle)
                {
                    kinectSensor.ElevationAngle += 5;
                }
        }

        private void btnSensorDown_Click(object sender, RoutedEventArgs e)
        {
            if (kinectSensor.ElevationAngle != kinectSensor.MinElevationAngle) {
                kinectSensor.ElevationAngle-=5;
            }
        }

        private void btnSensorMin_Click(object sender, RoutedEventArgs e)
        {
            kinectSensor.ElevationAngle = kinectSensor.MinElevationAngle;
        }

        private void btnSensorMax_Click(object sender, RoutedEventArgs e)
        {
            kinectSensor.ElevationAngle = kinectSensor.MaxElevationAngle;
        }

        private void btnBernardButton_Click(object sender, RoutedEventArgs e)
        {
            points = new GeometryModel3D[640*480];
            pc = true;

            //cube mesh viewer
            modeller = new ScannerModeller(realDepthCollection, this.width, this.height, new MeshGeometry3D());
            model = modeller.RenderKinectPoints();

            //triangle mesh viewer
            modeller = new ScannerModeller(vpcanvas, points);
            points = modeller.RenderKinectPointsTriangle();

        }

        //Viewport manipulation

        /*private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!mDown) return;
            Point pos = Mouse.GetPosition(bodyviewport);
            Point actualPos = new Point(
                    pos.X - bodyviewport.ActualWidth / 2,
                    bodyviewport.ActualHeight / 2 - pos.Y);
            double dx = actualPos.X - mLastPos.X;
            double dy = actualPos.Y - mLastPos.Y;
            double mouseAngle = 0;

            if (dx != 0 && dy != 0)
            {
                mouseAngle = Math.Asin(Math.Abs(dy) /
                    Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
                if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
                else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
                else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
            }
            else if (dx == 0 && dy != 0)
            {
                mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
            }
            else if (dx != 0 && dy == 0)
            {
                mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;
            }

            double axisAngle = mouseAngle + Math.PI / 2;

            Vector3D axis = new Vector3D(
                    Math.Cos(axisAngle) * 4,
                    Math.Sin(axisAngle) * 4, 0);

            double rotation = 0.02 *
                    Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

            Transform3DGroup group = model.Transform as Transform3DGroup;
            QuaternionRotation3D r =
                 new QuaternionRotation3D(
                 new Quaternion(axis, rotation * 180 / Math.PI));
            group.Children.Add(new RotateTransform3D(r));

            mLastPos = actualPos;

        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            mDown = true;
            Point pos = Mouse.GetPosition(bodyviewport);
            mLastPos = new Point(
                    pos.X - bodyviewport.ActualWidth / 2,
                    bodyviewport.ActualHeight / 2 - pos.Y);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mDown = false;
        }*/


        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
           bodycamera.Position = new Point3D(
           bodycamera.Position.X,
           bodycamera.Position.Y,
           bodycamera.Position.Z - 0.5);
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (kinectConnected)
            {
                this.kinectSensor.Stop();
            }
        }

        private void btnRobinButton_Click(object sender, RoutedEventArgs e)
        {
            //enable point cloud generation, istantiate point cloud class  
            this.generatePC = true; 
            this.pointCloud = new ICP.PointCloud(this.width, this.height);

            /*
            pointCloud.setX();
            pointCloud.setY();
            pointCloud.setZ();
            pointCloud.init();
             */

        }
    }
}