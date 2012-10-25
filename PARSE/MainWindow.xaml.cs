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

        const float MaxDepth = 4095;
        const float MinDepth = 400;
        const float DepthOffset = MaxDepth - MinDepth;
        KinectSensor newsensor;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            newsensor.DepthStream.Enable();
            newsensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(newSensor_AllFramesReady);

            try
            {
                newsensor.Start();
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("No device connected or there is an error in the program");
            }
        }

        void newSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                { return; }
 
                Byte[] pixels = GenerateDepthBytes(depthFrame);
 
                Int32 stride = depthFrame.Width * 4;

                depthimage.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
            }
        }

        public static Byte CalculateDepthIntensity(Int32 dist)
        {
            return (Byte)(255 - (255 * Math.Max(DepthOffset - MinDepth, 0) / (MaxDepth)));
        }

        private Byte[] GenerateDepthBytes(DepthImageFrame depthFrame)
        {
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            Byte[] pixels = new Byte[depthFrame.Height * depthFrame.Width * 4];

            const Int32 BlueIndex = 0;
            const Int32 GreenIndex = 1;
            const Int32 RedIndex = 2;

            for (Int32 depthIndex = 0, colorIndex = 0; depthIndex < rawDepthData.Length && colorIndex < pixels.Length; depthIndex++, colorIndex += 4)
            {
                Int32 player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
                Int32 depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                Byte intensity = CalculateDepthIntensity(depth);
                pixels[colorIndex + BlueIndex] = intensity;
                pixels[colorIndex + GreenIndex] = intensity;
                pixels[colorIndex + RedIndex] = intensity;
            }

            return pixels;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(newsensor);
        }

        void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.AudioSource.Stop();
            }
        }

    }
}
