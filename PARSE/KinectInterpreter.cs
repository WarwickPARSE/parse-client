using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        public KinectInterpreter()
        {
            kinectReady = false;
            rgbReady = false;
            depthReady = false;
            skeletonReady = false;
            
            //Only try to use the Kinect sensor if there is one connected
            if (KinectSensor.KinectSensors.Count != 0)
            {
                this.kinectReady = true;
                
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
            this.kinectSensor.SkeletonStream.Enable();
            this.kinectSensor.Start();
            this.skeletonReady = true;
            this.kinectStatus = this.kinectStatus + ", Skeleton Ready";
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

                        this.outputBitmap = new WriteableBitmap(
                        depthFrame.Width,
                        depthFrame.Height,
                        96, // DpiX
                        96, // DpiY
                        PixelFormats.Bgr32,
                        null);

                        depthFrame.CopyPixelDataTo(this.depthPixelData);

                        byte[] convertedDepthBits = this.ConvertDepthFrame(this.depthPixelData, ((KinectSensor)sender).DepthStream);

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
        
    }
}