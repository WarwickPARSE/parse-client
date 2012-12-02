using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.GPU;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;


namespace PARSE
{
    public class BasicClassifierTester
    {
        // C:\\PARSE\\Training\\1\\results
        String relativeURI = Environment.CurrentDirectory;
        String classifierURI = "C:\\PARSE\\Training\\1\\results\\cascade.xml";

        IntPtr classifier;
        IntPtr memory;
        IntPtr resultsPtr;

        int stride = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        public BasicClassifierTester()
        {
            /*
                try
                {
                    Console.WriteLine(" - - - Trying to read file...");
                    using (StreamReader sr = new StreamReader(classifierURI))
                    {
                        String line = sr.ReadToEnd();
                        Console.WriteLine(" - - - Successfully read file!!");
                        Console.WriteLine(line);
                        sr.Close();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(" - - - The file could not be read:");
                    Console.WriteLine(e.Message);
                }
            */
            /*
            try
            {
                memory = CvInvoke.cvCreateMemStorage(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Inner Exception! - - - " + ex.InnerException);
            }
            Console.WriteLine("Memory allocated");
            classifier = CvInvoke.cvLoad(classifierURI, memory, "", new IntPtr());
            Console.WriteLine("Classifier loaded");
            */
        }

        public void classify(BitmapSource frame)
        {
            Console.WriteLine(relativeURI);

            //byte[] classifiedImage = frame;
            //WriteableBitmap frameImage = new WriteableBitmap(frameWidth, frameHeight, 96, 96, PixelFormats.Bgr32, null);

            //BitmapSource frameImage = BitmapSource.Create(frameWidth, frameHeight, 96, 96, PixelFormats.Bgr32, null, frame, stride);

            /*
            resultsPtr = CvInvoke.cvHaarDetectObjects(
                Marshal.GetIUnknownForObject(frame),
                classifier,
                resultsPtr,
                1.1,
                3,
                Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                new System.Drawing.Size(0,0),
                new System.Drawing.Size(0,0)
            );

            Console.WriteLine("Classified?!? Pointer below: ");
            Console.WriteLine(resultsPtr.ToString());
            */
            //return classifiedImage;
            Console.WriteLine(" - - - Converting Bitmap...");
            System.Drawing.Bitmap bitmapFrame;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(frame));
                enc.Save(outStream);
                bitmapFrame = new System.Drawing.Bitmap(outStream);
            }
            Console.WriteLine(" - - - Bitmap converted!");

            Image<Bgr, Byte> image = new Image<Bgr, Byte>(bitmapFrame);

            Console.WriteLine(" - - - Image set");
            Console.WriteLine(" - - - Check CUDA...");

            if (GpuInvoke.HasCuda)
            {
                Console.WriteLine(" - - - Has CUDA!");
                using (GpuCascadeClassifier target = new GpuCascadeClassifier(classifierURI))
                {
                    using (GpuImage<Bgr, Byte> gpuImage = new GpuImage<Bgr, byte>(image))
                    using (GpuImage<Gray, Byte> gpuGray = gpuImage.Convert<Gray, Byte>())
                    {
                        Console.WriteLine(" - - - Detecting!");
                        Rectangle[] targetSet = target.DetectMultiScale(gpuGray, 1.1, 10, System.Drawing.Size.Empty);
                        Console.WriteLine(" - - - Detected :D :D :D Printing rectangle set: ");
                        foreach (Rectangle f in targetSet)
                        {
                            Console.WriteLine("Rectangle found at: " + f.ToString());
                            //draw the face detected in the 0th (gray) channel with blue color
                            image.Draw(f, new Bgr(System.Drawing.Color.Blue), 2);
                        }
                        Console.WriteLine(" - - - DONE");
                    }
                }


            }
            else
            {

                using (HOGDescriptor des = new HOGDescriptor())
                {
                    //des.SetSVMDetector
                }

                Console.WriteLine(" - - - No CUDA  :( ");
                Console.WriteLine(" - - - Devices available: " + GpuInvoke.GetCudaEnabledDeviceCount());
            }
        }
    }
}