﻿using System;
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
    /// Interaction logic for CoreLoader.xaml
    /// </summary>
    public partial class CoreLoader : Window
    {

        //RGB Constants
        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        //Depth point array and frame definitions
        private short[] pixelData;
        private byte[] depthFrame32;
        private WriteableBitmap outputBitmap;
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private DepthImageFormat lastImageFormat;

        //RGB point array and frame definitions
        private byte[] colorpixelData;
        private byte[] colorFrameRGB;
        private WriteableBitmap outputColorBitmap;
        private ColorImageFormat rgbImageFormat;

        private bool kinectConnected = false;

        //Kinect sensor
        KinectSensor kinectSensor;

        public CoreLoader()
        {
            InitializeComponent();

            //Only try to use the Kinect sensor if there is one connected
            if (KinectSensor.KinectSensors.Count != 0)
            {
                kinectConnected = true;

                //Initialize sensors
                kinectSensor = KinectSensor.KinectSensors[0];

                //Enable streams
                kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                //Start streams
                kinectSensor.Start();

                //Check if streams are ready
                kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);
                kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);

            }
            else {
                lblStatus.Content = "Status: No Kinect device detected";
            }

        }

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

                //byte Distance = 0;

                //int MinimumDistance = 800;
                // MaximumDistance = 4096;
 
                if (realDepth < 800)
                {
                    this.depthFrame32[i32 + RedIndex] = 75;
                    this.depthFrame32[i32 + GreenIndex] = 0;
                    this.depthFrame32[i32 + BlueIndex] = 0;
                }
                else if (realDepth < 1000)
                {
                    this.depthFrame32[i32 + RedIndex] = 150;
                    this.depthFrame32[i32 + GreenIndex] = 0;
                    this.depthFrame32[i32 + BlueIndex] = 0;
                }
                else if (realDepth >= 1000 && realDepth < 1500)
                {
                    this.depthFrame32[i32 + RedIndex] = 240;
                    this.depthFrame32[i32 + GreenIndex] = 100;
                    this.depthFrame32[i32 + BlueIndex] = 0;
                }
                else if (realDepth >= 1500 && realDepth < 2000)
                {
                    this.depthFrame32[i32 + RedIndex] = 240;
                    this.depthFrame32[i32 + GreenIndex] = 100;
                    this.depthFrame32[i32 + BlueIndex] = 50;
                }
                else if (realDepth >= 2000 && realDepth < 2500)
                {
                    this.depthFrame32[i32 + RedIndex] = 240;
                    this.depthFrame32[i32 + GreenIndex] = 150;
                    this.depthFrame32[i32 + BlueIndex] = 100;
                }
                else if (realDepth >= 2500 && realDepth < 3000)
                {
                    this.depthFrame32[i32 + RedIndex] = 240;
                    this.depthFrame32[i32 + GreenIndex] = 200;
                    this.depthFrame32[i32 + BlueIndex] = 150;
                }
                else if (realDepth >= 3000 && realDepth < 3500)
                {
                    this.depthFrame32[i32 + RedIndex] = 140;
                    this.depthFrame32[i32 + GreenIndex] = 150;
                    this.depthFrame32[i32 + BlueIndex] = 150;
                }
                else if (realDepth >= 3500 && realDepth < 4000)
                {
                    this.depthFrame32[i32 + RedIndex] = 100;
                    this.depthFrame32[i32 + GreenIndex] = 100;
                    this.depthFrame32[i32 + BlueIndex] = 200;
                }
                else if (realDepth >= 4000 && realDepth < 4500)
                {
                    this.depthFrame32[i32 + RedIndex] = 50;
                    this.depthFrame32[i32 + GreenIndex] = 50;
                    this.depthFrame32[i32 + BlueIndex] = 200;
                }
                else
                {
                    this.depthFrame32[i32 + RedIndex] = 50;
                    this.depthFrame32[i32 + GreenIndex] = 50;
                    this.depthFrame32[i32 + BlueIndex] = 50;
                }
            }

            return this.depthFrame32;
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (kinectConnected) {
                this.kinectSensor.Stop();
            }
        }

        //TODO: prevent the following two methods from crashing if called in quick succession
        private void btnSensorUp_Click(object sender, RoutedEventArgs e)
        {
            if (kinectSensor.ElevationAngle != kinectSensor.MaxElevationAngle) {
                kinectSensor.ElevationAngle+=5;
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
    }
}