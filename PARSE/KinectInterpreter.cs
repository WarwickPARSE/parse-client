using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

//Kinect Imports
using Microsoft.Kinect;

namespace PARSE
{
    class KinectInterpreter
    {
        public KinectSensor                             kinectSensor { get; private set; }
        public string                                   kinectStatus { get; private set; }

        public bool                                     kinectReady { get; private set; }//true if kinect ready
        private bool                                    rgbReady;//ditto
        private bool                                    depthReady;//ditto
        private bool                                    skeletonReady;//ditto

        //RGB point array and frame definitions
        private byte[]                                  colorpixelData;
        private ColorImageFormat                        rgbImageFormat;
        private byte[]                                  colorFrameRGB;
        private WriteableBitmap                         outputColorBitmap;

        //Depth point array and frame definitions
        private short[]                                 depthPixelData;
        private byte[]                                  depthFrame32;
        private WriteableBitmap                         outputBitmap;
        private DepthImageFormat                        depthImageFormat;
        public int[]                                    realDepthCollection;
        public int                                      realDepth;

        //Skeleton point array and frame definitions
        private Skeleton[]                              skeletonData;
        private Dictionary<int, SkeletonFigure>         skeletons;
        private Canvas                                  skeletonCanvas;

        //Visualisation definitions
        private GeometryModel3D[]                       pts;
        private bool                                    visActive;

        private float                                   skelDepth;
        private float skelL;
        private float skelR;

        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        public KinectInterpreter(Canvas c)
        {
            kinectReady = false;
            rgbReady = false;
            depthReady = false;
            skeletonReady = false;
            
            //Only try to use the Kinect sensor if there is one connected
            if (KinectSensor.KinectSensors.Count != 0)
            {
                this.kinectReady = true;
                this.skeletonCanvas = c;
                
                //Initialize sensor
                this.kinectSensor = KinectSensor.KinectSensors[0];
                this.kinectStatus = "Initialized";
            }
        }

        //Enable depthStream
        public void startDepthStream()
        {
            this.kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            this.kinectSensor.Start();
            this.depthReady = true;
            this.kinectStatus = this.kinectStatus+", Depth Ready";
        }

        //Enable depthMeshStream
        public void startDepthMeshStream(GeometryModel3D[] pts)
        {
            this.kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            this.kinectSensor.Start();
            this.pts = pts;
            this.depthReady = true;
            visActive = true;
        }

        //Enable rgbStream
        public void startRGBStream()
        {
            this.kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            this.kinectSensor.Start();
            this.rgbReady = true;
            this.kinectStatus = this.kinectStatus + ", RGB Ready";
        }

        //enable skeletonStream
        public void startSkeletonStream()
        {
            skeletonData = new Skeleton[6];
            skeletons = new Dictionary<int, SkeletonFigure>();

            this.kinectSensor.SkeletonStream.Enable();
            this.kinectSensor.Start();
            this.skeletonReady = true;
            this.kinectStatus = this.kinectStatus + ", Skeleton Ready";
        }


        //Disable all streams on changeover
        public void stopStreams(String feedChoice)
        {

            //visActive set to false to stop duplicate visualisations
            visActive = false;

            //skelDepth needs to reset to 0 for switching between depth and depth+skel
            //skelDepth = -1;

            switch (feedChoice) {
                
                case "RGB + Skeletal":
                    this.kinectSensor.DepthStream.Disable();
                    this.kinectStatus = this.kinectStatus + ", Skeleton Ready";
                    break;

                case "Depth + Skeletal":
                    this.kinectSensor.ColorStream.Disable();
                    this.kinectStatus = this.kinectStatus + ", Skeleton Ready";
                    break;

                case "Skeletal":
                    this.kinectSensor.DepthStream.Disable();
                    this.kinectSensor.ColorStream.Disable();
                    this.kinectStatus = "Initialized, Skeleton Ready";
                    break;

                default:
                    this.kinectSensor.ColorStream.Disable();
                    this.kinectSensor.DepthStream.Disable();
                    this.kinectSensor.SkeletonStream.Disable();
                    this.kinectStatus = "Initialized";
                    break;
            }
        }

        public WriteableBitmap ColorImageReady(object sender, ColorImageFrameReadyEventArgs e)
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
                    }

                    colorFrame.CopyPixelDataTo(this.colorpixelData);

                    this.outputColorBitmap.WritePixels(new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height), colorpixelData, colorFrame.Width * Bgr32BytesPerPixel, 0);
                    
                    //this.rgbImageFormat = colorFrame.Format;

                    return this.outputColorBitmap;
                }
                
                return null;
            }
        
        }

        public WriteableBitmap DepthImageReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    bool NewFormat = this.depthImageFormat != depthFrame.Format;
                    if (NewFormat)
                    {
                        this.depthPixelData = new short[depthFrame.PixelDataLength];
                        this.depthFrame32 = new byte[depthFrame.Width * depthFrame.Height * Bgr32BytesPerPixel];
                        int temp = 0;
                        int i = 0;

                        this.outputBitmap = new WriteableBitmap(
                        depthFrame.Width,
                        depthFrame.Height,
                        96, // DpiX
                        96, // DpiY
                        PixelFormats.Bgr32,
                        null);

                        depthFrame.CopyPixelDataTo(this.depthPixelData);

                        byte[] convertedDepthBits = this.ConvertDepthFrame(this.depthPixelData, ((KinectSensor)sender).DepthStream);

                        if (visActive)
                        {
                            for (int a = 0; a < 480; a += 4)
                            {
                                for (int b = 0; b < 640; b += 4)
                                {
                                    temp = ((ushort)this.depthPixelData[b + a * 640]) >> 3;
                                    ((TranslateTransform3D)pts[i].Transform).OffsetZ = temp;
                                    i++;
                                }
                            }

                        }
                        else
                        {
                            this.outputBitmap.WritePixels(
                            new Int32Rect(0, 0, depthFrame.Width, depthFrame.Height),
                            convertedDepthBits,
                            depthFrame.Width * Bgr32BytesPerPixel,
                            0);
                        }

                        return this.outputBitmap;
                    }
                 }
                return null;
            }
        }

        public void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
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
                            skeletonFigure = new SkeletonFigure(this.skeletonCanvas);
                            skeletons.Add(trackedSkeleton.TrackingId, skeletonFigure);
                            Canvas.SetTop(this.skeletonCanvas, 0);
                            Canvas.SetLeft(this.skeletonCanvas, 0);
                        }

                        // Update the drawing
                        skelDepth = trackedSkeleton.Position.Z;
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
                var point = new Point((int)colorPoint.X / 640.0 * this.skeletonCanvas.ActualWidth, (int)colorPoint.Y / 480.0 * this.skeletonCanvas.ActualHeight);
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

                if (skelDepth < 0)
                {
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
                else
                {
                    if ((((skelDepth * 1000) - 100) <= realDepth) && (realDepth < ((skelDepth * 1000) + 100)))
                    {
                        this.depthFrame32[colorPixelIndex++] = 255;
                        this.depthFrame32[colorPixelIndex++] = 255;
                        this.depthFrame32[colorPixelIndex++] = 255;
                        ++colorPixelIndex;
                    }
                    else 
                    {
                        this.depthFrame32[colorPixelIndex++] = 0;
                        this.depthFrame32[colorPixelIndex++] = 0;
                        this.depthFrame32[colorPixelIndex++] = 0;
                        ++colorPixelIndex;
                    }
                }
            }
            skelDepth = -1;
            return this.depthFrame32;
        }
        
    }
}