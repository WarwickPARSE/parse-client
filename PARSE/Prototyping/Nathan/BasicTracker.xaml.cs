//System imports
using System;
using System.Collections.Generic;
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
        private int width = 640;
        private int height = 480;

        // Kinecty things
        private bool kinectConnected = false;
        public int[] realDepthCollection;
        public int realDepth;
        public int x;
        public int y;
        public int s = 4;
        public bool pc = false;

        private static volatile bool takeFrame = true;

        // Frame counter
        //public Thread frameProcessorThread;
        public int frames = 0;

        //Kinect sensor
        KinectSensor kinectSensor;

        public BasicTracker()
        {
            Console.WriteLine("Basic Tracker started");

            InitializeComponent();

            Console.WriteLine("Component Initialised!");

            //Only try to use the Kinect sensor if there is one connected
            if (KinectSensor.KinectSensors.Count != 0)
            {
                kinectConnected = true;

                //Initialize sensor
                kinectSensor = KinectSensor.KinectSensors[0];

                Console.WriteLine("Starting colour stream..");

                //Enable streams
                kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                Console.WriteLine("Starting kinect");

                //Start streams
                kinectSensor.Start();

                Console.WriteLine("Attaching frameready event...");

                //Check if streams are ready
                //TODO: there is no justification for isolating these events, it makes life much harder
                //kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);
                kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(run);

                statusbarStatus.Content = "Status: Device connected";

                //frameProcessorThread = new Thread(fpsCounter);
                //frameProcessorThread.Start();
            }
            else
            {
                statusbarStatus.Content = "Status: No Kinect device detected";
            }

        }


        //private void ColorImageReady(object sender, ColorImageFrameReadyEventArgs e)
        private void ProcessFrame(byte[] byteFrame)
        {
            Console.WriteLine("Color image ready!!");

            //using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (byteFrame != null)
                {
                    bool colorFormat = true; // this.rgbImageFormat != colorFrame.Format;

                    if (colorFormat)
                    {
                        //width = colorFrame.Width;
                        //height = colorFrame.Height;

                        //this.colorpixelData = new byte[colorFrame.PixelDataLength];
                        this.colorpixelData = new byte[byteFrame.Length];
                        this.colorFrameRGB = new byte[this.width * this.height * Bgr32BytesPerPixel];

                        this.outputColorBitmap = new WriteableBitmap(this.width, this.height, 96, 96, PixelFormats.Bgr32, null);
                        this.frameGrab.Source = this.outputColorBitmap;
                        //this.frameGrab.OpacityMask = new ImageBrush { ImageSource = 

                        this.processedBitmap = new WriteableBitmap(this.width, this.height, 96, 96, PixelFormats.Bgr32, null);
                        this.procImage.Source = this.processedBitmap;

                        this.redBitmap = new WriteableBitmap(this.width, this.height, 96, 96, PixelFormats.Gray8, null);
                        this.rgbImage_RED.Source = this.redBitmap;

                        this.greenBitmap = new WriteableBitmap(this.width, this.height, 96, 96, PixelFormats.Gray8, null);
                        this.rgbImage_GREEN.Source = this.greenBitmap;

                        this.blueBitmap = new WriteableBitmap(this.width, this.height, 96, 96, PixelFormats.Gray8, null);
                        this.rgbImage_BLUE.Source = this.blueBitmap;

                        Console.WriteLine("Frame written?");
                    }

                    //colorFrame.CopyPixelDataTo(this.colorpixelData)
                    colorpixelData = byteFrame;

                    // PROCESS THE DATA //
                    processedcolorpixelData = frameProcessor(colorpixelData);
                    processedcolorpixelData = findFeatures(processedcolorpixelData);
                    //targetFinder(colorpixelData);

                    // Output raw image
                    this.outputColorBitmap.WritePixels(
                        (new Int32Rect(0, 0, width, height)),
                        colorpixelData,
                        width * Bgr32BytesPerPixel,
                        0 );

                    // Output processed image
                    this.processedBitmap.WritePixels(
                        new Int32Rect(0, 0, width, height),
                        processedcolorpixelData,
                        width * Bgr32BytesPerPixel,
                        0);

                    // Output component images
                    this.redBitmap.WritePixels(
                        new Int32Rect(0, 0, width, height),
                        redComponent,
                        width * 1,
                        0);
                    this.greenBitmap.WritePixels(
                        new Int32Rect(0, 0, width, height),
                        greenComponent,
                        width * 1,
                        0);
                    this.blueBitmap.WritePixels(
                        new Int32Rect(0, 0, width, height),
                        blueComponent,
                        width * 1,
                        0);
                 
                    //this.rgbImageFormat = colorFrame.Format;
                    // ?!
                    frames++;

                }
            }
        }

        private byte[] targetFinder(byte[] colorpixelData)
        {
            byte[] mappedImage = colorpixelData;

            // Clean the image
            for (int i = 1; i < (colorpixelData.Length / 4) - 1; i += 1)
            {
                int index = i * 4;
                {
                    byte pixel = mappedImage[index];
                    if (pixel == 1)
                        if (mappedImage[index - 2] == 0 && mappedImage[index + 4] == 0)
                            mappedImage[index] = 0;
                }
            }
            return mappedImage;
        }

        private byte[] frameProcessor(byte[] image)
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
            }

            return processedcolorpixelData;
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

            Console.Write("Relative coords: " + xCoordinate + ", " + yCoordinate);
            int target = Convert.ToInt32((width * (yCoordinate - 1) + xCoordinate) * 4);

            Console.Write(colorpixelData[target+2] + " - " + colorpixelData[target+1] + " - " + colorpixelData[target]);
            rgbLabel_GET.Content = colorpixelData[target+2] + "-" + colorpixelData[target+1] + "-" + colorpixelData[target];
        }

        private void fpsCounter()
        {
           /* while (true)
            {
                Thread.Sleep(1000);
                int temp = frames;
                frames = 0;
                //statusbarFPS.Content = "FPS: " + frames.ToString();
                Console.WriteLine("FPS: " + frames.ToString());
            }
            */
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
                if (rgbSlider_range.Value == 0)
                    rgbSlider_range.Value = 40;
            }
        }

        private void frameGrab_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
        }

        private void run(object sender, AllFramesReadyEventArgs e)
        {

            if (!takeFrame)
            {
                try { e.OpenColorImageFrame().Dispose(); }
                catch (Exception noFrameException) { };
                takeFrame = true;
                return;
            }
            else
            {
                takeFrame = false;
            }

            byte[][] RGBWithMask = SensorAllFramesReady(sender, e);
            if (RGBWithMask[0] == null | RGBWithMask[1] == null)
                return;

            // Apply the mask
            byte[] MaskedRGB = new byte[RGBWithMask[1].Length];
            for (int i = 0; i < RGBWithMask[0].Length; i++)
            {
                if (RGBWithMask[0][i] == 0)
                {
                    int index = i * 4;
                    MaskedRGB[index] = RGBWithMask[1][index];
                    MaskedRGB[index + 1] = RGBWithMask[1][index + 1];
                    MaskedRGB[index + 2] = RGBWithMask[1][index + 2];
                    MaskedRGB[index + 3] = 0;
                }
                else
                {
                    int index = i * 4;
                    MaskedRGB[index] = 0;
                    MaskedRGB[index + 1] = 0;
                    MaskedRGB[index + 2] = 0;
                    MaskedRGB[index + 3] = 0;
                }
            }

            ProcessFrame(MaskedRGB);
        }

        private byte[][] SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            bool depthReceived = false;
            bool colorReceived = false;

            DepthImagePixel[] depthPixels;
            byte[] colorPixels;
            ColorImagePoint[] colorCoordinates;
            int colorToDepthDivisor;
            byte[] greenScreenPixelData;

            // Allocate space to put the color pixels we'll create
            depthPixels = new DepthImagePixel[this.kinectSensor.DepthStream.FramePixelDataLength];
            colorPixels = new byte[this.kinectSensor.ColorStream.FramePixelDataLength];
            greenScreenPixelData = new byte[this.kinectSensor.DepthStream.FramePixelDataLength];
            colorCoordinates = new ColorImagePoint[this.kinectSensor.DepthStream.FramePixelDataLength];

            int colorWidth = this.kinectSensor.ColorStream.FrameWidth;
            int colorHeight = this.kinectSensor.ColorStream.FrameHeight;
            colorToDepthDivisor = colorWidth / 640;

            byte[][] results = new byte[2][]; // kinectSensor.DepthStream.FramePixelDataLength];

            DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;
            ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (null != depthFrame)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(depthPixels);
                    depthReceived = true;
                }
            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (null != colorFrame)
                {
                    // Copy the pixel data from the image to a temporary array
                    this.outputColorBitmap = new WriteableBitmap(640, 480, 96, 96, PixelFormats.Bgr32, null);
                    colorFrame.CopyPixelDataTo(colorPixels);
                    colorReceived = true;
                }
            }

            if (true == depthReceived)
            {
                this.kinectSensor.CoordinateMapper.MapDepthFrameToColorFrame(
                    DepthFormat,
                    depthPixels,
                    ColorFormat,
                    colorCoordinates);

                Array.Clear(greenScreenPixelData, 0, greenScreenPixelData.Length);

                // loop over each row and column of the depth
                for (int y = 0; y < 480; ++y)
                {
                    for (int x = 0; x < 640; ++x)
                    {
                        // calculate index into depth array
                        int depthIndex = x + (y * 640);

                        DepthImagePixel depthPixel = depthPixels[depthIndex];

                        int player = depthPixel.PlayerIndex;

                        // if we're tracking a player for the current pixel, do green screen
                        if (player > 0)
                        {
                            // retrieve the depth to color mapping for the current depth pixel
                            ColorImagePoint colorImagePoint = colorCoordinates[depthIndex];

                            // scale color coordinates to depth resolution
                            int colorInDepthX = colorImagePoint.X / colorToDepthDivisor;
                            int colorInDepthY = colorImagePoint.Y / colorToDepthDivisor;

                            // make sure the depth pixel maps to a valid point in color space
                            if (colorInDepthX > 0 && colorInDepthX < 640 && colorInDepthY >= 0 && colorInDepthY < 480)
                            {
                                // calculate index into the green screen pixel array
                                int greenScreenIndex = colorInDepthX + (colorInDepthY * 640);

                                // set opaque
                                greenScreenPixelData[greenScreenIndex] = 33;

                                // compensate for depth/color not corresponding exactly by setting the pixel 
                                // to the left to opaque as well
                                greenScreenPixelData[greenScreenIndex - 1] = 33;
                            }
                        }
                    }
                }
            }

            if (true == colorReceived)
            {
                // Write the pixel data into our bitmap
                /*
                this.outputColorBitmap.WritePixels(
                new Int32Rect(0, 0, this.outputColorBitmap.PixelWidth, this.outputColorBitmap.PixelHeight),
                colorPixels,
                this.outputColorBitmap.PixelWidth * sizeof(int),
                0);

                if (playerOpacityMaskImage == null)
                {
                    playerOpacityMaskImage = new WriteableBitmap(
                        640,
                        480,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null);

                    results[0] = playerOpacityMaskImage;
                }

                playerOpacityMaskImage.WritePixels(
                    new Int32Rect(0, 0, 640, 480),
                    greenScreenPixelData,
                    640 * ((playerOpacityMaskImage.Format.BitsPerPixel + 7) / 8),
                    0);
                */
                results[0] = greenScreenPixelData; // playerOpacityMaskImage
                results[1] = colorPixels;
                return results;
            }

            return results;
        }

    private byte[] findFeatures(byte[] image)
    {
        // Feature map to return
        byte[] featureMap = new byte [image.Length];

        // Features
        int featurePixelCount = 0;
        int centroidX = 0;
        int centroidY = 0;
        List<int> xList = new List<int>();
        List<int> yList = new List<int>();

        // Get row pointers
        int[] rowHeaders = new int[this.height];
        for (int row = 0; row < this.height; row++)
        {
            rowHeaders[row] = row * this.width * 4;
        }

        // Convolve!
        for (int row = 0; row < this.height - 4; row++)
        {
            for (int column = 0; column < this.width - 4; column++)
            {
                int sum = 0;
                for (int x = column; x <= column + 4; x++)
                    for (int y = row; y <= row + 4; y++)
                    {
                        if (image[(rowHeaders[y] + (x*4) )] > 0)
                            sum += 10;
                    }

                // Threshold the image
                if (sum > 50)
                {
                    //Console.WriteLine(sum);

                    int index = rowHeaders[row + 2] + (column * 4) + (8); // 8 = (2px * 4 bytes per pixel)
                    featureMap[index] = 255;//sum
                    featureMap[index + 1] = 255; // sum;
                    featureMap[index + 2] = 255; // sum
                    featureMap[index + 3] = 255; //sum

                    featurePixelCount++;
                    xList.Add(column + 2);
                    yList.Add(row + 2);
                }
            }
        }

        if (featurePixelCount > 0)
        {
            //centroidX = featureX / featurePixelCount;
            xList.ForEach(delegate(int element)
            {
                centroidX += element;
            });
            centroidX = centroidX / featurePixelCount;
            yList.ForEach(delegate(int element)
            {
                centroidY += element;
            });
            centroidY = centroidY / featurePixelCount;

            for (int row = centroidY - 4; row < centroidY + 4; row++)
                for (int col = centroidX - 4; col < centroidX + 4; col++)
                {
                    if (row > 0 & col > 0 & row < 480 & col < 640)
                    {
                        int index = rowHeaders[row] + col * 4;
                        featureMap[index] = 0;
                        featureMap[index + 1] = 255;
                        featureMap[index + 2] = 0;
                        featureMap[index + 3] = 0;
                    }
                }

            Matrix inertiaMatrix = new Matrix();

            IEnumerator<int> xListEnumerator = xList.GetEnumerator();
            IEnumerator<int> yListEnumerator = yList.GetEnumerator();
            
            while (xListEnumerator.MoveNext() & yListEnumerator.MoveNext())
            {
                int dx = xListEnumerator.Current - centroidX;
                int dy = yListEnumerator.Current - centroidY;

                inertiaMatrix.add(new Matrix(
                    dx * dx,
                    dx * dy,
                    dx * dy,
                    dy * dy
                ));
            }

            // Remove?
            inertiaMatrix.normalise();

            // Perform eigen analysis!
            Matrix eigenMatrix = new Matrix();
            double trace = inertiaMatrix.value[0,0] + inertiaMatrix.value[1,1];
            double determinant = (inertiaMatrix.value[0, 0] * inertiaMatrix.value[1, 1]) - (inertiaMatrix.value[0, 1] * inertiaMatrix.value[1, 0]);
            
            double eigenvalue1b = (trace  + Math.Pow(((Math.Pow(trace, 2)) / (4 - determinant)), 0.5)) / 2;
            double eigenvalue2b = (trace / 2) - Math.Pow(((Math.Pow(trace, 2)) / (4 - determinant)), 0.5);

            double eigenvalue1 = (trace + Math.Pow(Math.Pow(trace, 2) - 4 * determinant, 0.5)) / 2;
            double eigenvalue2 = (trace - Math.Pow(Math.Pow(trace, 2) - 4 * determinant, 0.5)) / 2;

            if (inertiaMatrix.value[1, 0] == 0 & inertiaMatrix.value[1, 0] == 0)
            {
                eigenMatrix = new Matrix(1, 0, 0, 1);
            }
            else if (inertiaMatrix.value[1, 0] != 0)
            {
                eigenMatrix.value[0, 0] = eigenvalue1 - inertiaMatrix.value[1, 1];
                eigenMatrix.value[1, 0] = inertiaMatrix.value[1, 0];
                eigenMatrix.value[0, 1] = eigenvalue2 - inertiaMatrix.value[1, 1];
                eigenMatrix.value[1, 1] = inertiaMatrix.value[1, 0];

                eigenMatrix.normalise();
            }
            else if (inertiaMatrix.value[0, 1] != 0)
            {
                eigenMatrix.value[0, 0] = inertiaMatrix.value[0, 1];
                eigenMatrix.value[1, 0] = eigenvalue1 - inertiaMatrix.value[0, 0];
                eigenMatrix.value[0, 1] = inertiaMatrix.value[0, 1];
                eigenMatrix.value[1, 1] = eigenvalue2 - inertiaMatrix.value[0, 0];

                eigenMatrix.normalise();
            }
            else
                throw new Exception("Examine zero checks in eigenanalysis");

            bool hasAngle = ((2 * eigenMatrix.value[0, 1]) == eigenMatrix.value[1, 1]  & (eigenMatrix.value[0,0] == eigenMatrix.value[1,1]));
            double top = eigenMatrix.value[0, 1] * 2  ;
            double bottom = (eigenMatrix.value[0, 0] - eigenMatrix.value[1, 1]);
            double angle = 0;
            /*
            if (bottom != 0 & top != 0)
            {
                angle = Math.Atan(top / bottom);
                angle = angle / 2;
            }
            */
            // Compare vector magnitudes where magnitude = sqrt(val1^2 + val2^2)
            if (
                    (Math.Pow( 
                        (
                        Math.Pow(eigenMatrix.value[0, 0], 2)
                        + 
                        Math.Pow(eigenMatrix.value[1, 0], 2)
                        )
                        , 0.5)
                    )
                     > 
                    (Math.Pow(
                        (
                        Math.Pow(eigenMatrix.value[0, 1], 2)
                        +
                        Math.Pow(eigenMatrix.value[1, 1], 2)
                        )
                        , 0.5)
                    )
                )
            {
                // Left vector is the larger
                // Angle = tan^-1 y/x
                angle = Math.Atan(eigenMatrix.value[0,0]/eigenMatrix.value[1,0]);
                if (angle < 0)
                    angle += Math.PI / 2;
                else
                    angle -= Math.PI / 2;
                Console.WriteLine("Left");
            }
            else
            {
                // Right vector is the larger
                // Angle = tan^-1 y/x
                angle = Math.Atan(eigenMatrix.value[0,1]/eigenMatrix.value[1,1]);
                
                Console.WriteLine("Right");
            }
            Target_Coordinate_Label.Content = "hasAngle: " + hasAngle + "  angle: " + angle;
            Console.WriteLine("Eigen:  " + eigenMatrix.ToString() + "  -> e1: " + eigenvalue1 + ", e2: " + eigenvalue2 + " -> top: " + top + ", bottom: " + bottom);
        }
        else
        {
            Target_Coordinate_Label.Content = "Target centroid at: -none-";
        }
        return featureMap; 
    }
    }
}