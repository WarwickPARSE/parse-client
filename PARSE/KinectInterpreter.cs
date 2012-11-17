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
using System.Threading;
using System.Threading.Tasks;

using Emgu.CV;
using HelixToolkit.Wpf;

//Kinect Imports
using Microsoft.Kinect;

namespace PARSE
{
    class KinectInterpreter
    {
        public KinectSensor                             kinectSensor { get; private set; }
        public string                                   kinectStatus { get; private set; }

        public bool                                     kinectReady { get; private set; }//true if kinect ready
        public bool                                     IsColorStreamUpdating { get; set; }
        public bool                                     IsDepthStreamUpdating { get; set; }
        private bool                                    rgbReady;//ditto
        private bool                                    depthReady;//ditto
        private bool                                    skeletonReady;//ditto

        //RGB point array and frame definitions
        private byte[]                                  colorpixelData;
        private ColorImageFormat                        rgbImageFormat;
        private WriteableBitmap                         outputColorBitmap;

        //Depth point array and frame definitions
        private short[]                                 depthPixelData;
        private Byte[]                                  depthFrame32;
        public Point3D[]                                depthFramePoints;
        public Point[]                                  textureCoordinates;
        private WriteableBitmap                         outputBitmap;
        private DepthImageFormat                        depthImageFormat;
        public GeometryModel3D                          Model;
        public GeometryModel3D[]                        pts;
        public int[]                                    rawDepth;
        private bool                                    updateColorData;
        private bool                                    updateDepthData;
       
        //Skeleton point array and frame definitions
        private Skeleton[]                              skeletonData;
        private Dictionary<int, SkeletonFigure>         skeletons;
        private Canvas                                  skeletonCanvas;
        private float skelDepth;
        private float skelDepthDelta = 125;//to be used if we ever implement sliders so we can scan fat people, shouldnt really exceed 255/2
        private float skelL;
        private float skelLDelta = 0;//to be used if we ever implement sliders so we can scan fat people
        private float skelR;
        private float skelRDelta = 0;//to be used if we ever implement sliders so we can scan fat people 

        //Visualisation definitions
        private int                                     visMode;
        private DiffuseMaterial                         imageMesh;

        //Constants
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        public KinectInterpreter(Canvas c)
        {
            kinectReady = false;
            rgbReady = false;
            depthReady = false;
            skeletonReady = false;

            Model = new GeometryModel3D();
            CompositionTarget.Rendering += this.CompositionTarget_Rendering;

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

        //updates the 3d composition target
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            this.updateColorData = true;
            this.updateDepthData = true;
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
            this.pts = pts;
            this.kinectSensor.Start();
            this.depthReady = true;
            visMode = 1;
        }

        public void startDepthLinearStream(GeometryModel3D mod)
        {
            this.kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            this.kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            this.kinectSensor.Start();

            this.Model = mod;
            this.depthReady = true;
            visMode = 2;

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
            visMode = 0;

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
                        this.outputColorBitmap = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                    }

                    colorFrame.CopyPixelDataTo(this.colorpixelData);

                    this.outputColorBitmap.WritePixels(new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height), colorpixelData, colorFrame.Width * Bgr32BytesPerPixel, 0);

                    //Updates texture for point cloud visualisation
                    imageMesh = new DiffuseMaterial(new ImageBrush(this.outputColorBitmap));
                    this.Model.Material = this.Model.BackMaterial = imageMesh;

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
                        int i = 0;
                        int temp = 0;
                        this.depthPixelData = new short[depthFrame.PixelDataLength];
                        this.depthFrame32 = new byte[depthFrame.Width * depthFrame.Height * Bgr32BytesPerPixel];
                        this.depthFramePoints = new Point3D[depthFrame.PixelDataLength];
                        this.textureCoordinates = new Point[depthFrame.PixelDataLength];
                        this.rawDepth = new int[depthFrame.PixelDataLength];

                        this.outputBitmap = new WriteableBitmap(
                        depthFrame.Width,
                        depthFrame.Height,
                        96, // DpiX
                        96, // DpiY
                        PixelFormats.Bgr32,
                        null);

                        depthFrame.CopyPixelDataTo(this.depthPixelData);
                        byte[] convertedDepthBits = this.ConvertDepthFrame(this.depthPixelData, ((KinectSensor)sender).DepthStream);

                        //VIS MODE 1: Feedback to visualisation with Z offset.
                        if (visMode==1)
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

                        //VIS MODE 2: Feedback to visualisation with colour map.
                        else if (visMode == 2)
                        {

                            //BUG: offset between colour and depth feed is out by 50px - fix.

                            for (int a = 0; a < depthFrame.Height; a++)
                            {
                                for (int b = 0; b < depthFrame.Width; b++)
                                {
                                    this.textureCoordinates[a * depthFrame.Width + b]
                                        = new Point((double)b / (depthFrame.Width - 1), (double)a
                                            / (depthFrame.Height - 1));
                                }
                            }

                            if (!this.Model.IsFrozen)
                            {
                                this.Model.Geometry = this.CreateMesh(depthFrame.Width, depthFrame.Height);
                                //this.Model.Freeze();
                            }

                        }

                       this.outputBitmap.WritePixels(
                            new Int32Rect(0, 0, depthFrame.Width, depthFrame.Height),
                            convertedDepthBits,
                            depthFrame.Width * Bgr32BytesPerPixel,
                            0);

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

                        //update the depth of the tracked skeleton
                        skelDepth = trackedSkeleton.Position.Z;
                        skelL = trackedSkeleton.Joints[JointType.HandLeft].Position.X;
                        skelR = trackedSkeleton.Joints[JointType.HandRight].Position.X;

                        skelDepth = skelDepth * 1000;
                        skelL = (320 * (1 + skelL)) * 4;
                        skelR = (320 * (1 + skelR)) * 4;

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
        /// 

        private MeshGeometry3D CreateMesh(int width, int height, double depthDifferenceTolerance = 200)
        {
            var triangleIndices = new List<int>();
            for (int iy = 0; iy + 1 < height; iy++)
            {
                for (int ix = 0; ix + 1 < width; ix++)
                {
                    int i0 = (iy * width) + ix;
                    int i1 = (iy * width) + ix + 1;
                    int i2 = ((iy + 1) * width) + ix + 1;
                    int i3 = ((iy + 1) * width) + ix;

                    var d0 = this.rawDepth[i0];
                    var d1 = this.rawDepth[i1];
                    var d2 = this.rawDepth[i2];
                    var d3 = this.rawDepth[i3];

                    var dmax0 = Math.Max(Math.Max(d0, d1), d2);
                    var dmin0 = Math.Min(Math.Min(d0, d1), d2);
                    var dmax1 = Math.Max(d0, Math.Max(d2, d3));
                    var dmin1 = Math.Min(d0, Math.Min(d2, d3));

                    if (dmax0 - dmin0 < depthDifferenceTolerance && dmin0 != -1)
                    {
                        triangleIndices.Add(i0);
                        triangleIndices.Add(i1);
                        triangleIndices.Add(i2);
                    }

                    if (dmax1 - dmin1 < depthDifferenceTolerance && dmin1 != -1)
                    {
                        triangleIndices.Add(i0);
                        triangleIndices.Add(i2);
                        triangleIndices.Add(i3);
                    }
                }
            }

            return new MeshGeometry3D()
            {
                Positions = new Point3DCollection(this.depthFramePoints),
                TextureCoordinates = new System.Windows.Media.PointCollection(this.textureCoordinates),
                TriangleIndices = new Int32Collection(triangleIndices)
            };
        }

        private byte[] ConvertDepthFrame(short[] depthFrame, DepthImageStream depthStream)
        {
            int tooNearDepth = depthStream.TooNearDepth;
            int tooFarDepth = depthStream.TooFarDepth;
            int unknownDepth = depthStream.UnknownDepth;

            int cx = depthStream.FrameWidth / 2;
            int cy = depthStream.FrameHeight / 2;

            double fxinv = 1.0 / 476;
            double fyinv = 1.0 / 476;

            double scale = 0.001;

            this.rawDepth = new int[depthFrame.Length];

            if (visMode == 2)
            {

                for(int iy = 0; iy < 480; iy++)
                {
                    for (int ix = 0; ix < 640; ix++)
                    {
                        int i = (iy * 640) + ix;
                        this.rawDepth[i] = depthFrame[(iy * 640) + ix] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                        if (rawDepth[i] == unknownDepth || rawDepth[i] < tooNearDepth || rawDepth[i] > tooFarDepth)
                        {
                            this.rawDepth[i] = -1;
                            this.depthFramePoints[i] = new Point3D();
                        }
                        else
                        {
                            double zz = this.rawDepth[i] * scale;
                            double x = (cx - ix) * zz * fxinv;
                            double y = zz;
                            double z = (cy - iy) * zz * fyinv;
                            this.depthFramePoints[i] = new Point3D(x, y, z);
                        }
                    }
                }

            }

            else
            {

                int colorPixelIndex = 0;
                for (int i = 0; i < depthFrame.Length; i++)
                {

                    this.rawDepth[i] = depthFrame[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                    if (skelDepth < 0)
                    {
                        if (rawDepth[i] < 1066)
                        {
                            this.depthFrame32[colorPixelIndex++] = (byte)(255 * rawDepth[i] / 1066);
                            this.depthFrame32[colorPixelIndex++] = 0;
                            this.depthFrame32[colorPixelIndex++] = 0;
                            ++colorPixelIndex;

                        }
                        else if ((1066 <= rawDepth[i]) && (rawDepth[i] < 2133))
                        {
                            this.depthFrame32[colorPixelIndex++] = (byte)(255 * (2133 - rawDepth[i]) / 1066);
                            this.depthFrame32[colorPixelIndex++] = (byte)(255 * (rawDepth[i] - 1066) / 1066);
                            this.depthFrame32[colorPixelIndex++] = 0;
                            ++colorPixelIndex;
                        }
                        else if ((2133 <= rawDepth[i]) && (rawDepth[i] < 3199))
                        {
                            this.depthFrame32[colorPixelIndex++] = 0;
                            this.depthFrame32[colorPixelIndex++] = (byte)(255 * (3198 - rawDepth[i]) / 1066);
                            this.depthFrame32[colorPixelIndex++] = (byte)(255 * (rawDepth[i] - 2133) / 1066);
                            ++colorPixelIndex;
                        }
                        else if (3199 <= rawDepth[i])
                        {
                            this.depthFrame32[colorPixelIndex++] = 0;
                            this.depthFrame32[colorPixelIndex++] = 0;
                            this.depthFrame32[colorPixelIndex++] = (byte)(255 * (4000 - rawDepth[i]) / 801);
                            ++colorPixelIndex;
                        }
                    }
                    else
                    {
                        if ((((skelDepth - skelDepthDelta) <= rawDepth[i]) && (rawDepth[i] < (skelDepth + skelDepthDelta))) && (((skelL - skelLDelta) <= (colorPixelIndex % 2560)) && ((colorPixelIndex % 2560) < (skelR + skelRDelta))))
                        {
                            this.depthFrame32[colorPixelIndex++] = (byte)(255 * (rawDepth[i] - skelDepth - skelDepthDelta) / (2 * skelDepthDelta));
                            this.depthFrame32[colorPixelIndex++] = (byte)(255 * (rawDepth[i] - skelDepth - skelDepthDelta) / (2 * skelDepthDelta));
                            this.depthFrame32[colorPixelIndex++] = (byte)(255 * (rawDepth[i] - skelDepth - skelDepthDelta) / (2 * skelDepthDelta));
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

            }
            return this.depthFrame32;
        }

    }
}