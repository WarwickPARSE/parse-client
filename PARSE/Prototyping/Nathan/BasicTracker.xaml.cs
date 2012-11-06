//System imports


using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//Kinect imports
using Microsoft.Kinect;

namespace PARSE.Prototyping.Nathan
{
    public partial class BasicTracker : Window
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
        private WriteableBitmap processedBitmap; 
        private ColorImageFormat rgbImageFormat;

        //frame sizes
        private int width;
        private int height;

        // Kinecty things
        private bool kinectConnected = false;
        public int[] realDepthCollection;
        public int realDepth;
        public int x;
        public int y;
        public int s = 4;
        public bool pc = false;

        // Frame counter
        public Thread frameProcessorThread;

        //Kinect sensor
        KinectSensor kinectSensor;

        public BasicTracker()
        {
            InitializeComponent();

            //Only try to use the Kinect sensor if there is one connected
            if (KinectSensor.KinectSensors.Count != 0)
            {
                kinectConnected = true;

                //Initialize sensor
                kinectSensor = KinectSensor.KinectSensors[0];

                //Enable streams
                kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                //Start streams
                kinectSensor.Start();

                //Check if streams are ready
                //TODO: there is no justification for isolating these events, it makes life much harder
                kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);

                statusbarStatus.Content = "Status: Device connected";
            }
            else
            {
                statusbarStatus.Content = "Status: No Kinect device detected";
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
                        this.frameGrab.Source = this.outputColorBitmap;

                        this.processedBitmap = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                        this.procImage.Source = this.processedBitmap;
                    }

                    colorFrame.CopyPixelDataTo(this.colorpixelData);

                    // Output raw image to frameGrab
                    this.outputColorBitmap.WritePixels(
                        (new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height)),
                        colorpixelData,
                        colorFrame.Width * Bgr32BytesPerPixel,
                        0 );

                    // Output processed image to procImage
                    this.processedBitmap.WritePixels(
                        new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                        frameProcessor(colorpixelData, colorFrame.Width, colorFrame.Height),
                        colorFrame.Width * Bgr32BytesPerPixel,
                        0 );

                    this.rgbImageFormat = colorFrame.Format;
                }
            }
        }

        private byte[] frameProcessor(byte[] image, int width, int height)
        {
            byte[] processedImage = new byte [width * height * 4];

            double redMin    = rgbSlider_RED.Value - 10;
            double redMax    = rgbSlider_RED.Value + 10;
            double greenMin  = rgbSlider_GREEN.Value - 10;
            double greenMax  = rgbSlider_GREEN.Value + 10;
            double blueMin   = rgbSlider_BLUE.Value - 10;
            double blueMax   = rgbSlider_BLUE.Value + 10;

            Console.Write("image received! sending to thread... \n " + image);

            for (int i = 0; i < image.Length; i += 4)
            {
                // See where things are!
                if (
                    image[i] > redMin & image[i] < redMax &
                    image[i + 1] > greenMin & image[i + 1] < greenMax &
                    image[i + 2] > blueMin & image[i + 2] < blueMax
                    )
                {
                    processedImage[i] = 255;
                    processedImage[i + 1] = 255;
                    processedImage[1 + 2] = 255;
                    processedImage[i + 3] = image[i + 3];
                }

                processedImage[i] = 255;

                /*
                //GREEN
                processedImage[i + 1] = image[i + 1];
                //BLUE
                processedImage[i + 2] = 0;
                //ALPHA
                processedImage[i + 3] = 0;
                */
            }

            return processedImage;
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (kinectConnected)
            {
                this.kinectSensor.Stop();
            }
        }

        private void procImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Console.Write("procImage_ImageFailed exception called! Any idea what caused that? " + e); 
        }

        private void RGBSlider_GREEN_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void rgbSlider_RED_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void rgbSlider_BLUE_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }


    }
}