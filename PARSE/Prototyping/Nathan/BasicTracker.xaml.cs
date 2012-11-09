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
        //Depth point array and frame definitions
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        // Composite RGB images - input & output
        private byte[] colorpixelData;
        private byte[] processedcolorpixelData;
        private byte[] colorFrameRGB;
        private WriteableBitmap outputColorBitmap;
        private WriteableBitmap processedBitmap; 
        private ColorImageFormat rgbImageFormat;

        // RGB Component images
        private byte[] redComponent;
        private byte[] greenComponent;
        private byte[] blueComponent;
        private WriteableBitmap redBitmap;
        private WriteableBitmap greenBitmap;
        private WriteableBitmap blueBitmap;

        //RGB Constants
        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

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
        public int frames = 0;

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

                frameProcessorThread = new Thread(fpsCounter);
                frameProcessorThread.Start();
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
                        width = colorFrame.Width;
                        height = colorFrame.Height;

                        this.colorpixelData = new byte[colorFrame.PixelDataLength];
                        this.colorFrameRGB = new byte[colorFrame.Width * colorFrame.Height * Bgr32BytesPerPixel];

                        this.outputColorBitmap = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                        this.frameGrab.Source = this.outputColorBitmap;

                        this.processedBitmap = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                        this.procImage.Source = this.processedBitmap;

                        this.redBitmap = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Gray8, null);
                        this.rgbImage_RED.Source = this.redBitmap;

                        this.greenBitmap = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Gray8, null);
                        this.rgbImage_GREEN.Source = this.greenBitmap;

                        this.blueBitmap = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Gray8, null);
                        this.rgbImage_BLUE.Source = this.blueBitmap;
                    }

                    colorFrame.CopyPixelDataTo(this.colorpixelData);

                    // PROCESS THE DATA //
                    frameProcessor(colorpixelData);


                    // Output raw image
                    this.outputColorBitmap.WritePixels(
                        (new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height)),
                        colorpixelData,
                        colorFrame.Width * Bgr32BytesPerPixel,
                        0 );

                    // Output processed image
                    this.processedBitmap.WritePixels(
                        new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                        processedcolorpixelData,
                        colorFrame.Width * Bgr32BytesPerPixel,
                        0);

                    // Output component images
                    this.redBitmap.WritePixels(
                        new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                        redComponent,
                        colorFrame.Width * 1,
                        0);
                    this.greenBitmap.WritePixels(
                        new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                        greenComponent,
                        colorFrame.Width * 1,
                        0);
                    this.blueBitmap.WritePixels(
                        new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                        blueComponent,
                        colorFrame.Width * 1,
                        0);
                 
                    this.rgbImageFormat = colorFrame.Format;

                    frames++;

                }
            }
        }

        private void frameProcessor(byte[] image)
        {
            processedcolorpixelData = new byte [width * height * 4];
            redComponent = new byte[width * height];
            greenComponent = new byte[width * height];
            blueComponent = new byte[width * height];

            double range = rgbSlider_range.Value;
            double redMin    = rgbSlider_RED.Value - range;
            double redMax    = rgbSlider_RED.Value + range;
            double greenMin  = rgbSlider_GREEN.Value - range;
            double greenMax  = rgbSlider_GREEN.Value + range;
            double blueMin   = rgbSlider_BLUE.Value - range;
            double blueMax   = rgbSlider_BLUE.Value + range;

            

            for (int i = 0; i < (image.Length / 4); i+=1)
            {
                // See where things are!
                if (
                    image[i*4 + 2] > redMin & image[i*4 + 2] < redMax &
                    image[i*4 + 1] > greenMin & image[i*4 + 1] < greenMax &
                    image[i*4] > blueMin & image[i*4] < blueMax
                    )
                {
                    processedcolorpixelData[i*4] = 255;
                    processedcolorpixelData[i*4 + 1] = 255;
                    processedcolorpixelData[1*4 + 2] = 255;
                    processedcolorpixelData[i*4 + 3] = image[i*4 + 3];
                }


                //RED
                redComponent[i] = image[i*4+2];
                

                //GREEN
                greenComponent[i] = image[i*4 + 1];
                
                
                //BLUE
                blueComponent[i] = image[i*4];
                

                //ALPHA?
                
            }
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
            rgbLabel_Target.Content = Math.Floor(rgbSlider_RED.Value) + " - " + Math.Floor(rgbSlider_GREEN.Value) + " - " + Math.Floor(rgbSlider_BLUE.Value);
        }

        private void rgbSlider_RED_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            rgbLabel_Target.Content = Math.Floor(rgbSlider_RED.Value) + " - " + Math.Floor(rgbSlider_GREEN.Value) + " - " + Math.Floor(rgbSlider_BLUE.Value);
        }

        private void rgbSlider_BLUE_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            rgbLabel_Target.Content = Math.Floor(rgbSlider_RED.Value) + " - " + Math.Floor(rgbSlider_GREEN.Value) + " - " + Math.Floor(rgbSlider_BLUE.Value);
        }

        private void BasicTrackerWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void frameGrab_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void frameGrab_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.Write("\n");
            Console.Write(e.GetPosition(frameGrab));

            double xCoordinate = e.GetPosition(frameGrab).X / 253;
            double yCoordinate = e.GetPosition(frameGrab).Y / 187;

            xCoordinate = xCoordinate * 640;
            yCoordinate = yCoordinate * 480;

            xCoordinate = Math.Floor(xCoordinate);
            yCoordinate = Math.Floor(yCoordinate);

            Console.Write("   Relative crds: " + xCoordinate + ", " + yCoordinate);

            int target = Convert.ToInt32((width * (yCoordinate - 1) + xCoordinate) * 4);

            Console.Write(colorpixelData[target+2] + " - " + colorpixelData[target+1] + " - " + colorpixelData[target]);

            rgbLabel_GET.Content = colorpixelData[target+2] + "-" + colorpixelData[target+1] + "-" + colorpixelData[target];

        }

        private void fpsCounter()
        {
            while (true)
            {
                Thread.Sleep(1000);
                int temp = frames;
                frames = 0;
                //statusbarFPS.Content = "FPS: " + frames.ToString();
                Console.WriteLine("FPS: " + frames.ToString());

            }
        }

        private void btn_FindColour_Click(object sender, RoutedEventArgs e)
        {
            String labelContent = rgbLabel_GET.Content.ToString();
            if (labelContent != "Click image to get RGB")
            {
                String[] RGB = labelContent.Split('-');
                rgbSlider_RED.Value = Double.Parse(RGB[0]);
                rgbSlider_GREEN.Value = Double.Parse(RGB[1]);
                rgbSlider_BLUE.Value = Double.Parse(RGB[2]);
                rgbSlider_range.Value = 40;
            }
        }




        


    }
}