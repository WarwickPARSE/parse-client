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

            byte hb = 0;
            byte sb = 0;
            byte lb = 0;

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
                    //interpreter.kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);
                    kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);

                    Console.WriteLine("Done! Waiting for frames...");

                    //while (true)
                    {
                        //BitmapSource nextFrameBS = interpreter.getRGBTexture();
                        //if (nextFrameBS != null)
                        //procImage.Source = nextFrame;
                        /*
                        System.Threading.Thread.BeginCriticalRegion();
                        ColorImageFrame currentFrame = nextFrame;
                        processFrame(currentFrame);
                        System.Threading.Thread.EndCriticalRegion();
                        */
                    }
                }
                else
                {
                    Console.WriteLine("Status: No Kinect device detected");
                }
            }


          private void ColorImageReady(object sender, ColorImageFrameReadyEventArgs e)
          {
              //Console.WriteLine("frame ready!");

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

                      // Output raw image
                      this.outputColorBitmap = new WriteableBitmap(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                      this.outputColorBitmap.WritePixels(
                          (new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height)),
                          colorpixelData,
                          colorFrame.Width * Bgr32BytesPerPixel,
                          0);
                      this.raw_image.Source = this.outputColorBitmap;

                      // PROCESS THE DATA //
                      colorpixelData = convertToHSL(colorpixelData);
                      frameProcessor(colorpixelData);
                      //processedcolorpixelData = colorpixelData;

                      // Output processed image
                      this.processedBitmap.WritePixels(
                          new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                          processedcolorpixelData,
                          colorFrame.Width * Bgr32BytesPerPixel,
                          0);

                      this.rgbImageFormat = colorFrame.Format;

                      this.procImage.Source = this.processedBitmap;
                      //Console.WriteLine("Frame written");

                      colorFrame.Dispose();
                  }

              }
          }

            private byte[] convertToHSL(byte[] rgbImage)
            {
                //Console.WriteLine("Converting to HSL");

                byte[] hslImage = new byte [width * height * 4];

                for (int i = 0; i < (rgbImage.Length / 4); i += 1)
                {
                    int index = i * 4;
                    float r = rgbImage[index + RedIndex];
                    float g = rgbImage[index + GreenIndex];
                    float b = rgbImage[index + BlueIndex];
                    float h = 0;
                    float s = 0;
                    float l = 0;

                    float mx = Math.Max(Math.Max(r, g), b);
                    float mn = Math.Min(Math.Min(r, g), b);
                    // imx?
                    l = (mx + mn) / 2;
                    if (mx - mn == 0)
                    {
                        s = 0;
                        h = 0;

                        hslImage[index] = (byte) (255 * h);
                        hslImage[index + 1] = (byte)(255 * s);
                        hslImage[index + 2] = (byte)(255 * l);

                        continue;
                    }

                    if (l < 0.5)
                        s = (mx - mn) / (mx + mn);
                    else
                        s = (mx - mn) / (2 - (mx + mn));

                    if (r == mx)
                        h = ((g - b) / (mx - mn)) / 6;
                    if (g == mx)
                        h = ((2 + (b - r)) / (mx - mn)) / 6;
                    if (b == mx)
                        h = ((4 + (r - g) / (mx - mn))) / 6;

                    if (h < 0)
                        h += 1;

                    hslImage[index] = (byte)(255 * h);
                    hslImage[index + 1] = (byte)(255 * s);
                    hslImage[index + 2] = (byte)(255 * l);

                }

                /*
                int HIndex = 0;
                int SIndex = 1;
                int LIndex = 2;
                
                //for (int i = 0; i < (rgbImage.Length / 4); i += 1)
                int i = 0;
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

                }
                */
                return hslImage;
                //return rgbImage;
            }

            private void frameProcessor(byte[] image)
            {
                //Console.WriteLine("Processing frame");

                processedcolorpixelData = new byte [width * height * 4];

                int hSlider = (int)slider1.Value;
                int sSlider = (int)slider2.Value;
                int lSlider = (int)slider3.Value;

                double range = range_slider.Value;

                double hMin = hSlider - range;// - 10;// *1 - range;
                double hMax = hSlider + range;// + 10;// *1 + range;
                hMax = Math.Min(hMax, 255);
                hMin = Math.Max(hMin, 0);
                double sMin = sSlider - range;// - 10;// *1 - range;
                double sMax = sSlider + range;//+ 10;// *1 + range;
                sMax = Math.Min(sMax, 255);
                sMin = Math.Max(sMin, 0);
                double lMin = lSlider - range;// - 10;// *1 - range;
                double lMax = lSlider + range;// + 10;// *1 + range;
                lMax = Math.Min(lMax, 255);
                lMin = Math.Max(lMin, 0);
            

                for (int i = 0; i < (image.Length / 4); i+=1)
                {
                    // See where things are!
                    if (
                        image[i*4] > hMin & image[i*4] < hMax &
                        image[i*4 + 1] > sMin & image[i*4 + 1] < sMax &
                        image[i*4 + 2] > lMin & image[i*4 + 2] < lMax
                        )
                    {
                        processedcolorpixelData[i*4] = 255;
                        processedcolorpixelData[i*4 + 1] = 255;
                        processedcolorpixelData[1*4 + 2] = 255;
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
                if (slider1.IsInitialized & slider2.IsInitialized & slider3.IsInitialized & HSL_Target_Label.IsInitialized)
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

            private void raw_image_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
            {

                double xCoordinate = e.GetPosition(raw_image).X / raw_image.Width;
                double yCoordinate = e.GetPosition(raw_image).Y / raw_image.Height;

                xCoordinate = xCoordinate * 640;
                yCoordinate = yCoordinate * 480;

                xCoordinate = Math.Floor(xCoordinate);
                yCoordinate = Math.Floor(yCoordinate);

                //Console.Write("   Relative crds: " + xCoordinate + ", " + yCoordinate);

                int target = Convert.ToInt32((width * (yCoordinate - 1) + xCoordinate) * 4);

                //Console.Write(colorpixelData[target + 2] + " - " + colorpixelData[target + 1] + " - " + colorpixelData[target]);

                //rgbLabel_GET.Content = colorpixelData[target + 2] + "-" + colorpixelData[target + 1] + "-" + colorpixelData[target];

                float r = colorpixelData[target + 2];
                float g = colorpixelData[target + 1];
                float b = colorpixelData[target];
                float h = 0;
                float s = 0;
                float l = 0;

                float mx = Math.Max(Math.Max(r, g), b);
                float mn = Math.Min(Math.Min(r, g), b);
                // imx?
                l = (mx + mn) / 2;
                if (mx - mn == 0)
                {
                    s = 0;
                    h = 0;

                    hb = (byte)(255 * h);
                    sb = (byte)(255 * s);
                    lb = (byte)(255 * l);

                    selected_target.Content = "Selected HSL: " + hb.ToString() + ", " + sb.ToString() + ", " + lb.ToString();
                    return;
                }

                if (l < 0.5)
                    s = (mx - mn) / (mx + mn);
                else
                    s = (mx - mn) / (2 - (mx + mn));

                if (r == mx)
                    h = ((g - b) / (mx - mn)) / 6;
                if (g == mx)
                    h = ((2 + (b - r)) / (mx - mn)) / 6;
                if (b == mx)
                    h = ((4 + (r - g) / (mx - mn))) / 6;

                if (h < 0)
                    h += 1;

                hb = (byte)(255 * h);
                sb = (byte)(255 * s);
                lb = (byte)(255 * l);
                selected_target.Content = "Selected HSL: " + hb.ToString() + ", " + sb.ToString() + ", " + lb.ToString();
            }

            private void selected_target_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                slider1.Value = hb;
                slider2.Value = sb;
                slider3.Value = lb;
            }
    }
}

