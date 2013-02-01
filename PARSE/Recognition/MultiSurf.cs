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
        string[] Model_Urls = new string[] {"Specs//Front_Lacoste_1.bmp", "Specs//Spine.bmp"};
        Image<Gray, byte>[] Models;

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

        public Image<Bgr, Byte> Draw(Image<Gray, byte> observedImage)
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

            SurfResults match = analyseResults(MultiSurfResults);
            resultsToDisk(match);

            return match.getMappedImage();
        }

        private void resultsToDisk(SurfResults match)
        {
            // Write log file
            
        }

        /// <summary>
        /// Analyses the SurfResults objects, and finds the object with the best match.
        /// </summary>
        /// <param name="MultiSurfResults"></param>
        /// <returns>SurfResults</returns>
        private SurfResults analyseResults(SurfResults[] MultiSurfResults)
        {
            int highestMatches = 0;
            SurfResults bestMatch = MultiSurfResults[0];

            // Find the result with the most matches
            foreach (SurfResults result in MultiSurfResults)
            {
                int matches = result.getMatches();
                if (matches > highestMatches)
                {
                    // Note the number of matches
                    highestMatches = matches;
                    // Note the result
                    bestMatch = result;
                }
            }

            if (highestMatches == 0)
            {
                Console.WriteLine("No matches found");

                Image<Bgr, byte> noMatchResults;

                try
                {
                    noMatchResults = new Image<Bgr, Byte>("C://PARSE//MultiSurf//No_Matches_Found.png");
                    bestMatch = new SurfResults(false, 0, MultiSurfResults[0].getOriginalImage(), noMatchResults, 0);
                }
                catch (Exception e)
                {
                    Console.WriteLine("MultiSURF - Exception whilst opening image file No_Matches_Found.png");
                    Console.WriteLine(e.InnerException);
                    Console.WriteLine(e.StackTrace);
                    throw e;
                }

            }

            return bestMatch;
        }

        private void runDetector(int index, Image<Gray, byte> observedImage)
        {
            //MultiSurfResults[detector] = detectors[detector].Draw(Models[detector], observedImage, out matchTime);
        }

    }
}
