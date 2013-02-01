using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.GPU;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using PARSE.Recognition;


namespace PARSE
{
    class MultiSurf
    {
        ///<summary>
        ///Takes an observed image and searches for matches with multiple reference images, using multiple SURF detectors
        ///</summary>

        // Model images
        string[] Model_Urls = new string[] {"Patterns/Checkerboard+trishapes/Checkerboard_four_trishapes.png"};
        Image<Gray, byte>[] Models;

        Image<Bgr, byte> MatchFailImage;

        // Target image
        string Target_Url = "C:/PARSE/MultiSurf/Patterns/Checkerboard+trishapes/Canyon.bmp";
        Image<Gray, byte> Target;

        // SURF Detectors
        int numberOfDetectors = 0;
        int currentDetectorIndex = 0;
        int mostSuccessfulDetector = 0;
        SurfDetector[] detectors;
        SurfResults[] MultiSurfResults;

        // Threading
        //Thread[] detector_threads;
        

        public MultiSurf()
        {
            // Load the model images into memory
            loadModels();

            // Instantiate detectors
            detectors = new SurfDetector[numberOfDetectors];
            for (int i = 0; i < numberOfDetectors; i++)
                detectors[i] = new SurfDetector();
            
            // Get the current detector
            //currentDetector = detectors[currentDetectorIndex];
            
            Console.WriteLine("MultiSurf Controller ready");
        }

        private void loadTarget()
        {
            Console.WriteLine("Opening target image...");
            Image<Bgr, Byte> inputImageRaw;
            try
            {
                //inputImageRaw = new Image<Bgr, Byte>("C:/PARSE/MultiSurf/Specs/Positives15.jpg");
                //inputImageRaw = new Image<Bgr, Byte>("C:/PARSE/Training/1/img/Positives7.jpg");
                inputImageRaw = new Image<Bgr, Byte>(Target_Url);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load target image.");
                Console.WriteLine(e.InnerException);
                throw e;
            }
            Image<Gray, Byte> inputImage = inputImageRaw.Convert<Gray, Byte>();

            Target = inputImage;

            Console.WriteLine("Opened target image.");

        }

        private void loadModels()
        {
            Models = new Image<Gray, byte>[Model_Urls.Length];
            numberOfDetectors = Model_Urls.Length;
            currentDetectorIndex = 0;

            Console.WriteLine("Loading model images...");
            for (int index = 0; index < Model_Urls.Length; index++)
            {
                try
                {
                    Models[index] = new Image<Bgr, Byte>("C://PARSE//MultiSurf//" + Model_Urls[index]).Convert<Gray, Byte>();
                }
                
                catch (Exception e)
                {
                    Console.WriteLine("MultiSURF - Exception whilst opening image file " + Model_Urls[index]);
                    Console.WriteLine(e.InnerException);
                    Console.WriteLine(e.StackTrace);
                    Environment.Exit(0);
                }
                Console.WriteLine("Loaded model " + Model_Urls[index]);
            }
            Console.WriteLine("Model images loaded");
        }

        public Image<Bgr, byte> run(Image<Gray, byte> targetImage)
        {
            if (targetImage != null)
                return Draw(targetImage);
            else
            {
                Console.Error.WriteLine("MultiSurf.run(Image<Gray, byte> targetImage) called but no image provided!");
                return run();
            }
        }

        public Image<Bgr, byte> run()
        {
            loadTarget();
            return DrawFromFile(Target);
        }
        

        private Image<Bgr, Byte> Draw(Image<Gray, byte> observedImage)
        {
            MultiSurfResults = new SurfResults[numberOfDetectors];

            
            for (int detector = 0; detector < numberOfDetectors; detector++)
            {
                //detector_threads[detector] = new Thread(new ThreadStart(runDetector(detector, observedImage)));
                long matchTime;
                MultiSurfResults[detector] = detectors[detector].Draw(Models[detector], observedImage, out matchTime);
            }

            //foreach (Thread thread in detector_threads)
                //thread.Start();

            int match = analyseResults(MultiSurfResults);
            resultsToDisk(MultiSurfResults, match);

            return MultiSurfResults[match].getMappedImage();
        }

        private Image<Bgr, Byte> DrawFromFile(Image<Gray, byte> observedImage)
        {
            MultiSurfResults = new SurfResults[numberOfDetectors];

            for (int detector = 0; detector < numberOfDetectors; detector++)
            {
                //detector_threads[detector] = new Thread(new ThreadStart(runDetector(detector, observedImage)));
                long matchTime;
                MultiSurfResults[detector] = detectors[detector].Draw(Models[detector], observedImage, out matchTime);
                MultiSurfResults[detector].setURLs(Model_Urls[detector], Target_Url);
            }

            //foreach (Thread thread in detector_threads)
            //thread.Start();

            int match = analyseResults(MultiSurfResults);

            resultsToDisk(MultiSurfResults, match);

            if (match > -1)
                return MultiSurfResults[match].getMappedImage();
            else
                return MatchFailImage;
        }

        private void resultsToDisk(SurfResults[] results, int bestMatch)
        {
            // Create a folder
            String homeFolder = "C:/PARSE/MultiSurf/Testing/Results/";
            // String timestamp = System.DateTime.Now.ToUniversalTime().ToString();
            String timestamp = string.Format("{0:yyyy-MM-dd_hh-mm-ss}", DateTime.Now);
            String finalDirectory = homeFolder + timestamp;
            Console.WriteLine("Date is: " + timestamp);
            System.IO.Directory.CreateDirectory(finalDirectory);

            // Log the details
            System.IO.StreamWriter logFileWriter = new System.IO.StreamWriter(finalDirectory + "/log.txt");
            logFileWriter.WriteLine("Log file for test run at: " + timestamp);
            logFileWriter.WriteLine("Target image: " + Target_Url);
            logFileWriter.WriteLine("");
            logFileWriter.WriteLine(" * * * * * * * * * * ");

            for (int index = 0; index < results.Length; index++)
            {
                SurfResults result = results[index];
                logFileWriter.WriteLine("Results for SURF instance " + index);
                logFileWriter.WriteLine("");

                string resultString = result.ToString();
                logFileWriter.WriteLine(resultString);

                logFileWriter.WriteLine(" * * * * * * * * * * ");

                // Save the image to disk
                result.getMappedImage().Save(finalDirectory + "/image-" + index.ToString() + ".bmp");
            }

            logFileWriter.Close();

        }

        /// <summary>
        /// Analyses the SurfResults objects, and finds the object with the best match.
        /// </summary>
        /// <param name="MultiSurfResults"></param>
        /// <returns>SurfResults</returns>
        private int analyseResults(SurfResults[] MultiSurfResults)
        {
            int highestMatches = 0;
            int bestMatch = 0;

            // Find the result with the most matches
            for (int index = 0; index < MultiSurfResults.Length; index++)
            //foreach (SurfResults result in MultiSurfResults)
            {
                SurfResults result = MultiSurfResults[index];

                int matches = result.getMatches();
                if (matches > highestMatches)
                {
                    // Note the number of matches
                    highestMatches = matches;
                    // Note the result
                    bestMatch = index;
                }
            }

            if (highestMatches < 5)
            {
                Console.WriteLine("No matches found");
                bestMatch = -1;
                noMatches();
            }

            return bestMatch;
        }

        private void runDetector(int index, Image<Gray, byte> observedImage)
        {
            //MultiSurfResults[detector] = detectors[detector].Draw(Models[detector], observedImage, out matchTime);
        }

        private void noMatches()
        {
            if (MatchFailImage == null)
            {
                try
                {
                    MatchFailImage = new Image<Bgr, Byte>("C://PARSE//MultiSurf//No_Matches_Found.png");
                }
                catch (Exception e)
                {
                    Console.WriteLine("MultiSURF - Exception whilst opening image file No_Matches_Found.png");
                    Console.WriteLine(e.InnerException);
                    Console.WriteLine(e.StackTrace);
                    throw e;
                }
            }
        }

    }
}
