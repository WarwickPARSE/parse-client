//System imports
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//Kinect imports
using Microsoft.Kinect;

namespace PARSE.Tracking
{
    public class RGBTracker
    {
        //Depth point array and frame definitions
        private  readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        // Composite RGB images - input & output
        private byte[] colorpixelData;
        private byte[] processedcolorpixelData;
        private byte[] colorFrameRGB;
        private WriteableBitmap processedBitmap;

        //frame sizes
        private int width = 640;
        private int height = 480;
        private int[] rowHeaders;

        // Sensor Position
        private int x;
        private int y;
        private double angle;

        public RGBTracker()
        {
            // Get row pointers
            rowHeaders = new int[height];
            for (int row = 0; row < height; row++)
            {
                rowHeaders[row] = row * width * 4;
            }
        }

        public void ProcessFrame(byte[] byteFrame, out int x, out int y, out double angle)
        {
            this.colorpixelData = new byte[byteFrame.Length];
            this.colorFrameRGB = new byte[this.width * this.height * Bgr32BytesPerPixel];
            this.processedBitmap = new WriteableBitmap(this.width, this.height, 96, 96, PixelFormats.Bgr32, null);

            colorpixelData = byteFrame;

            // PROCESS THE DATA //
            processedcolorpixelData = RGBProcessor(colorpixelData);
            processedcolorpixelData = FindSensor(processedcolorpixelData);

            x = this.x;
            y = this.y;
            angle = this.angle;

            //Console.WriteLine("RGBScanner returning: " + this.x + ", " + this.y);
        }

        private byte[] RGBProcessor(byte[] image)
        {
            processedcolorpixelData = new byte[width * height * 4];

            for (int i = 0; i < (image.Length / 4); i += 1)
            {
                if (
                    image[i * 4 + 2] > 150 & image[i * 4 + 2] < 210 &     // RED
                    image[i * 4 + 1] > 150 & image[i * 4 + 1] < 210 &   // GREEN
                    image[i * 4] > 25 & image[i * 4] < 80             // BLUE
                    )
                {
                    processedcolorpixelData[i * 4] = 255;
                    processedcolorpixelData[i * 4 + 1] = 255;
                    processedcolorpixelData[1 * 4 + 2] = 255;
                    processedcolorpixelData[i * 4 + 3] = image[i * 4 + 3];
                }

                // was 190, 190, 35 +- 35
                // Try 160, 160, 30 +- 30
            }

            return processedcolorpixelData;
        }

        private byte[] FindSensor(byte[] image)
        {
            // Feature map to return
            byte[] featureMap = new byte[image.Length];

            // Features
            int featurePixelCount = 0;
            int centroidX = 0;
            int centroidY = 0;
            List<int> xList = new List<int>();
            List<int> yList = new List<int>();

            // Convolve!
            for (int row = 0; row < height - 4; row++)
            {
                for (int column = 0; column < width - 4; column++)
                {
                    int sum = 0;
                    for (int x = column; x <= column + 4; x++)
                        for (int y = row; y <= row + 4; y++)
                        {
                            if (image[(rowHeaders[y] + (x * 4))] > 0)
                                sum += 10;
                        }

                    // Threshold the image
                    if (sum > 50)
                    {
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

            // New >10 Threshold in place, to help prevent noise.
            if (featurePixelCount > 1)
            {
                //centroidX = featureX / featurePixelCount;
                xList.ForEach(delegate(int element)
                {
                    centroidX += element;
                });
                centroidX = centroidX / featurePixelCount;
                this.x = centroidX;
                yList.ForEach(delegate(int element)
                {
                    centroidY += element;
                });
                centroidY = centroidY / featurePixelCount;
                this.y = centroidY;



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
                double trace = inertiaMatrix.value[0, 0] + inertiaMatrix.value[1, 1];
                double determinant = (inertiaMatrix.value[0, 0] * inertiaMatrix.value[1, 1]) - (inertiaMatrix.value[0, 1] * inertiaMatrix.value[1, 0]);

                double eigenvalue1b = (trace + Math.Pow(((Math.Pow(trace, 2)) / (4 - determinant)), 0.5)) / 2;
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

                bool hasAngle = ((2 * eigenMatrix.value[0, 1]) == eigenMatrix.value[1, 1] & (eigenMatrix.value[0, 0] == eigenMatrix.value[1, 1]));
                double top = eigenMatrix.value[0, 1] * 2;
                double bottom = (eigenMatrix.value[0, 0] - eigenMatrix.value[1, 1]);
                double angle = 0;

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
                    // Angle = tan^-1 y/x + correction
                    angle = Math.Atan(eigenMatrix.value[0, 0] / eigenMatrix.value[1, 0]);
                    if (angle < 0)
                        angle += Math.PI / 2;
                    else
                        angle -= Math.PI / 2;
                }
                else
                {
                    // Right vector is the larger
                    // Angle = tan^-1 y/x
                    angle = Math.Atan(eigenMatrix.value[0, 1] / eigenMatrix.value[1, 1]);
                }
            }
            else
            {
                this.x = 0;
                this.y = 0;
                this.angle = 0;
            }
            return featureMap;
        }

        private  byte[] ApplyDepthMask(byte[] rgb, short[] depth)
        {
            byte[][] RGBWithMask = CreateDepthMask(rgb, depth);
            if (RGBWithMask[0] == null | RGBWithMask[1] == null)
                return null;

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

            return MaskedRGB; ;
        }

        private  byte[][] CreateDepthMask(byte[] rgb, short[] depth)
        {
            /*
            bool depthReceived = false;
            bool colorReceived = false;

            short[] depthPixels;
            byte[] colorPixels;
            ColorImagePoint[] colorCoordinates;
            int colorToDepthDivisor;
            byte[] greenScreenPixelData;

            // Allocate space to put the color pixels we'll create
            //depthPixels = new DepthImagePixel[this.kinectSensor.DepthStream.FramePixelDataLength];
            //colorPixels = new byte[this.kinectSensor.ColorStream.FramePixelDataLength];
            greenScreenPixelData = new byte[this.kinectSensor.DepthStream.FramePixelDataLength];
            colorCoordinates = new ColorImagePoint[this.kinectSensor.DepthStream.FramePixelDataLength];

            int colorWidth = this.kinectSensor.ColorStream.FrameWidth;
            int colorHeight = this.kinectSensor.ColorStream.FrameHeight;
            colorToDepthDivisor = colorWidth / 640;

            byte[][] results = new byte[2][]; // kinectSensor.DepthStream.FramePixelDataLength];

            DepthImageFormat DepthFormat = DepthImageFormat.Resolution640x480Fps30;
            ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

            // Copy the pixel data from the image to a temporary array
            depthPixels = depth;
            depthReceived = true;
             
            // Copy the pixel data from the image to a temporary array
            this.outputColorBitmap = new WriteableBitmap(640, 480, 96, 96, PixelFormats.Bgr32, null);
            colorPixels = colorpixelData;
            colorReceived = true;
             

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
                results[0] = greenScreenPixelData; // playerOpacityMaskImage
                results[1] = colorPixels;
                return results;
            }

            return results;
             */
            return null;
        }

    }
}