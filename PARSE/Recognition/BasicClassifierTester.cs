using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.Util;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.IO;


namespace PARSE
{
    public class BasicClassifierTester
    {
        // C:\\PARSE\\Training\\1\\results
        String classifierURI = "\\Recognition\\Classifiers\\cascade.xml";
        
        IntPtr classifier;
        IntPtr memory;
        IntPtr resultsPtr;

        int stride = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        public BasicClassifierTester()
        {
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
        }

        public void classify(BitmapSource frame)
        {
            //byte[] classifiedImage = frame;
            //WriteableBitmap frameImage = new WriteableBitmap(frameWidth, frameHeight, 96, 96, PixelFormats.Bgr32, null);

            //BitmapSource frameImage = BitmapSource.Create(frameWidth, frameHeight, 96, 96, PixelFormats.Bgr32, null, frame, stride);

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

            //return classifiedImage;
        }


    }
}