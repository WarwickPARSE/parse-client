using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace PARSE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private short[] pixelData;
        private byte[] depthFrame32;

        private WriteableBitmap outputBitmap;
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        private DepthImageFormat lastImageFormat;

        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        KinectSensor kinectSensor;

        public MainWindow()
        {
            InitializeComponent();
        
            kinectSensor = KinectSensor.KinectSensors[0];

            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

            kinectSensor.Start();
    
            kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);

            kinectSensor.ElevationAngle = 0;
        }

        private void DepthImageReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame imageFrame = e.OpenDepthImageFrame())
            {

            if (imageFrame != null)
            {
                bool NewFormat = this.lastImageFormat != imageFrame.Format;

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

                this.outputBitmap.WritePixels(
                new Int32Rect(0, 0, imageFrame.Width, imageFrame.Height),
                convertedDepthBits,
                imageFrame.Width * Bgr32BytesPerPixel,
                0);

                this.lastImageFormat = imageFrame.Format;
            }

            else { }
            }
        }

        private byte[] ConvertDepthFrame(short[] depthFrame, DepthImageStream depthStream)
        {

            for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < this.depthFrame32.Length; i16++, i32 += 4)
            {

                int realDepth = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                byte Distance = 0;
                byte distf1 = 0;
                byte distf2 = 0;

                int MinimumDistance = 800;
                int MaximumDistance = 4096;

                if (realDepth > MinimumDistance)
                {

                    distf1 = (byte) (realDepth-MinimumDistance);
                    distf2 = (byte) (255 / (MaximumDistance-MinimumDistance));
                    Distance = (byte) (255 - (distf1 * distf2));
                   //Distance = (byte)(255-((realDepth – MinimumDistance) * 255 / (MaximumDistance – MinimumDistance)));

                    this.depthFrame32[i32 + RedIndex] = (byte)(Distance);
                    this.depthFrame32[i32 + GreenIndex] = (byte)(Distance);
                    this.depthFrame32[i32 + BlueIndex] = (byte)(Distance);

                }
                else
                {
                    this.depthFrame32[i32 + RedIndex] = 150;
                    this.depthFrame32[i32 + GreenIndex] = 0;
                    this.depthFrame32[i32 + BlueIndex] = 0;
                }
            }

            return this.depthFrame32;
        }
    }
}