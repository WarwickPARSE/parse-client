using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace PARSE.Prototyping.Nathan
{
    /// <summary>
    /// Interaction logic for HSLTracker.xaml
    /// </summary>
    public partial class HSLTracker : Window
    {
            //Depth point array and frame definitions
            private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

            KinectInterpreter interpreter;

            // Composite RGB images - input & output
            private ColorImageFrame nextFrame;
            private byte[] colorpixelData;
            private byte[] processedcolorpixelData;
            private byte[] colorFrameRGB;
            //private byte[] colorFrameHSL;
            private WriteableBitmap outputColorBitmap;
            private WriteableBitmap processedBitmap; 
            private ColorImageFormat rgbImageFormat;

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

            private bool takeFrame = true;

            // Frame counter
            //public Thread frameProcessorThread;
            public int frames = 0;
    
            //Kinect sensor
            KinectSensor kinectSensor;

            public HSLTracker()
            {
                Console.WriteLine("HSL Tracker started");

                InitializeComponent();

                //interpreter = new KinectInterpreter(null);

                Console.WriteLine("Component Initialised!");

                //Only try to use the Kinect sensor if there is one connected
                if (KinectSensor.KinectSensors.Count != 0)
                {
                    kinectConnected = true;

                    //Initialize sensor
                    kinectSensor = KinectSensor.KinectSensors[0];

                    Console.WriteLine("Starting colour stream..");

                    //Enable streams
                    //interpreter.startRGBStream();
                    kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30); //.RgbResolution1280x960Fps12);  //.RawBayerResolution1280x960Fps12);

                    Console.WriteLine("Starting kinect");                    
                    kinectSensor.Start();

                    Console.WriteLine("Attaching frameready event...");

                    //Check if streams are ready
                    //TODO: there is no justification for isolating these events, it makes life much harder
                    //interpreter.kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);
                    kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);

                    Console.WriteLine("Done! Waiting for frames...");

                    //while (true)
                    {
                        //BitmapSource nextFrameBS = interpreter.getRGBTexture();
                        //if (nextFrameBS != null)
                        //procImage.Source = nextFrame;
                        
                        System.Threading.Thread.BeginCriticalRegion();
                        ColorImageFrame currentFrame = nextFrame;
                        processFrame(currentFrame);
                        System.Threading.Thread.EndCriticalRegion();

                    }
                }
                else
                {
                    Console.WriteLine("Status: No Kinect device detected");
                }
            }


          private void ColorImageReady(object sender, ColorImageFrameReadyEventArgs e)
          {
              Console.WriteLine("frame ready!");
              if(this.nextFrame != null)
              {
                  this.nextFrame.Dispose();
              }
              ColorImageFrame nextFrame = e.OpenColorImageFrame();

              //processFrame(e.OpenColorImageFrame());
              processFrame(nextFrame);
          }

          private void processFrame(ColorImageFrame colorFrame)
          {
              //using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
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

                          this.processedBitmap = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null);

                          //this.procImage.Source = this.processedBitmap; // do now or later?

                          //Console.WriteLine("Frame written");
                      }

                      colorFrame.CopyPixelDataTo(this.colorpixelData);

                      

                      // PROCESS THE DATA //
                      //colorpixelData = convertToHSL(colorpixelData);
                      //frameProcessor(colorpixelData);
                      processedcolorpixelData = colorpixelData;

                      /*
                      // Output raw image
                      this.outputColorBitmap = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                      this.outputColorBitmap.WritePixels(
                          (new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height)),
                          colorpixelData,
                          colorFrame.Width * Bgr32BytesPerPixel,
                          0);
                      */

                      // Output processed image
                      this.processedBitmap.WritePixels(
                          new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                          processedcolorpixelData,
                          colorFrame.Width * Bgr32BytesPerPixel,
                          0);

                      this.rgbImageFormat = colorFrame.Format;

                      this.procImage.Source = this.processedBitmap;
                      Console.WriteLine("Frame written");

                      colorFrame.Dispose();
                  }

              }
          }

            private byte[] convertToHSL(byte[] rgbImage)
            {
                //Console.WriteLine("Converting to HSL");

                //byte[] hslImage = new byte [width * height * 4];

                int HIndex = 0;
                int SIndex = 1;
                int LIndex = 2;
                /*
                for (int i = 0; i < (rgbImage.Length / 4); i += 1)
                {
                    int index = i * 4;

                    byte r = rgbImage[(index + RedIndex)];
                    byte g = rgbImage[(index + GreenIndex)];
                    byte b = rgbImage[(index + BlueIndex)];

                    System.Drawing.Color drawColor = System.Drawing.Color.FromArgb(r, g, b);
                    hslImage[(index + HIndex)] = (byte)(drawColor.GetHue()*255);
                    hslImage[(index + SIndex)] = (byte)(drawColor.GetSaturation()*255);
                    hslImage[(index + LIndex)] = (byte)(drawColor.GetBrightness()*255);

                    hslImage[(index + HIndex)] = 128;
                    hslImage[(index + SIndex)] = 128;
                    hslImage[(index + LIndex)] = 128;

                }*/

                return rgbImage;
            }

            private void frameProcessor(byte[] image)
            {
                Console.WriteLine("Processing frame");

                processedcolorpixelData = new byte [width * height * 4];

                double range = range_slider.Value;
                double hMin = slider1.Value - 10;// *1 - range;
                double hMax = slider1.Value + 10;// *1 + range;
                hMax = Math.Min(hMax, 255);
                double sMin = slider2.Value - 10;// *1 - range;
                double sMax = slider2.Value + 10;// *1 + range;
                sMax = Math.Min(sMax, 100);
                double lMin = slider3.Value - 10;// *1 - range;
                double lMax = slider3.Value + 10;// *1 + range;
                lMax = Math.Min(lMax, 100);
            

                for (int i = 0; i < (image.Length / 4); i+=1)
                {
                    // See where things are!
                    if (
                        image[i*4 + 2] > hMin & image[i*4 + 2] < hMax &
                        image[i*4 + 1] > sMin & image[i*4 + 1] < sMax &
                        image[i*4] > lMin & image[i*4] < lMax
                        )
                    {
                        processedcolorpixelData[i*4] = 128;
                        processedcolorpixelData[i*4 + 1] = 128;
                        processedcolorpixelData[1*4 + 2] = 128;
                        processedcolorpixelData[i*4 + 3] = image[i*4 + 3];
                    }                
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

            private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                // Throws exception on program load if allowed to fire event
                if (slider1!=null & slider2!=null & slider3!=null)
                    HSL_Target_Label.Content = "Current target- h:" + slider1.Value + " s:" + slider2.Value + " l:" + slider3.Value;
            }

            private void slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                // Throws exception on program load if allowed to fire event
                if (slider2 != null & slider1!= null & slider3 != null)
                    HSL_Target_Label.Content = "Current target- h:" + slider1.Value + " s:" + slider2.Value + " l:" + slider3.Value;
            }

            private void slider3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                // Throws exception on program load if allowed to fire event
                if (slider3 != null & slider2 != null & slider1 != null & HSL_Target_Label != null)
                    HSL_Target_Label.Content = "Current target- h:" + slider1.Value + " s:" + slider2.Value + " l:" + slider3.Value;
            }

            private void BasicTrackerWindow_Loaded(object sender, RoutedEventArgs e)
            {
                Console.WriteLine("Window loaded!");
            }

            private void range_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                // Throws exception on program load if allowed to fire event
                if (range_slider != null & range_label!= null)
                    range_label.Content = "Variance: " + range_slider.Value;
            }
    }
}
