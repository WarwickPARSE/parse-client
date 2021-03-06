﻿using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Kinect;
using PARSE.Tracking;

namespace PARSE
{
    /// <summary>
    /// Interaction logic for MeasurementLoader.xaml
    /// </summary>
    public partial class MeasurementLoader : Window
    {
        // Reference to the tracking class.
        SensorTracker tracker;
        private enum CaptureModes {Capture_New, Capture_Existing};
        private int CaptureMode = 0;

        private SkeletonPosition sharedPosition;

        public MeasurementLoader()
        {
            System.Diagnostics.Debug.WriteLine("Measurement Window Starting...");

            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MeasurementLoader_Loaded);
        }

        //Event handlers for viewport interaction

        private void MeasurementLoader_Loaded(object Sender, RoutedEventArgs e)
        {
            //place relative to coreloader
            this.Top = this.Owner.Top + 70;
            this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - this.Width -20;

            if (KinectSensor.KinectSensors.Count > 0)
                System.Diagnostics.Debug.WriteLine("Measurement Window Ready");
            else
                System.Diagnostics.Debug.WriteLine("Measurement Window Ready, but no Kinect detected");
        }

        private void cancel_scan_Click(object sender, RoutedEventArgs e)
        {
            if (tracker != null)
                tracker.stop();

            start_scan.Visibility = Visibility.Visible;
            Visualisation.Visibility = Visibility.Collapsed;
            instructionblock.Text = "Measurement Cancelled\nClick below to try again";
            instructionblock2.Visibility = Visibility.Collapsed;
            instructionblock2.FontSize = 26;
            //this.Close();
        }

        private void start_scan_Click(object sender, RoutedEventArgs e)
        {
            this.CaptureMode = (int)CaptureModes.Capture_New;

            // hide buttons from form
            //cancel_scan.Visibility = Visibility.Collapsed;
            start_scan.Visibility = Visibility.Collapsed;
            rescanButton.Visibility = Visibility.Collapsed;
                        
            // show image & instructions
            Visualisation.Visibility = Visibility.Visible;
            instructionblock.Visibility = Visibility.Collapsed;
            instructionblock2.Text = "Loading...";
            instructionblock2.Visibility = Visibility.Visible;

            // TODO move the button to the edge but keep it visible
            cancel_scan.Visibility = Visibility.Hidden;
            
            System.Diagnostics.Debug.WriteLine("Starting measurement window...");

            // Start tracking
            tracker = new SensorTracker(Visualisation, this, instructionblock2);
            tracker.captureNewLocation();
            //tracker.captureAtLocation();
            
            // Hook up to the capture event, fired by the tracker.
            tracker.Capture += new SensorTracker.CaptureEventHandler(capture);
        }

        /// <summary>
        /// Capture and act upon the 'capture' event, fired by the tracker.
        /// Can use this method to actually collect a value from the sensor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="skel"></param>
        internal void capture(object sender, SkeletonPosition skel)
        {
            System.Diagnostics.Debug.WriteLine("Scan captured! END");

            if (this.CaptureMode == (int)CaptureModes.Capture_New)
            {
                // Write location and timestamp to the database
                
                //initialise database class
                DatabaseEngine db = new DatabaseEngine();

                DateTime timestamp = DateTime.Now;

                sharedPosition = skel;

                ((CoreLoader)(this.Owner)).savedLocation = sharedPosition;

                //add record to database
                //db.insertScanLocations("null", skel.jointName1, "null", skel.offsetXJ1, skel.offsetYJ1, "0", timestamp);

                Console.WriteLine("Writing to database! Values = " + skel.jointName1 + ", " + skel.offsetXJ1 + ", " + skel.offsetYJ1 + ", " + timestamp);
            }

            
            if (this.CaptureMode == (int)CaptureModes.Capture_Existing)
            {
                // Capture data from the scanner....
            }
           
            tracker.stop();
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (tracker != null)
            {
                tracker.stop();
                tracker.close();
            }
        }

        private void scan_existing_Click(object sender, RoutedEventArgs e)
        {
            this.CaptureMode = (int)CaptureModes.Capture_Existing;

            // hide buttons from form
            //cancel_scan.Visibility = Visibility.Collapsed;
            start_scan.Visibility = Visibility.Collapsed;
            rescanButton.Visibility = Visibility.Collapsed;

            // show image & instructions
            Visualisation.Visibility = Visibility.Visible;
            instructionblock.Visibility = Visibility.Collapsed;
            instructionblock2.Text = "Loading...";
            instructionblock2.Visibility = Visibility.Visible;

            // TODO move the button to the edge but keep it visible
            cancel_scan.Visibility = Visibility.Hidden;

            System.Diagnostics.Debug.WriteLine("Starting measurement window...");

            // Start tracking
            tracker = new SensorTracker(Visualisation, this, instructionblock2);
            //tracker.captureNewLocation();

            // Get a position from the database
            SkeletonPosition targetLocation = new SkeletonPosition();

            /*DatabaseEngine db = new DatabaseEngine();
            Tuple<int, String, double, double, DateTime> scanloc = db.getLatestScanLoc();

            targetLocation.jointName1 = scanloc.Item2;
            targetLocation.offsetXJ1 = scanloc.Item3;
            targetLocation.offsetYJ1 = scanloc.Item4;*/

            targetLocation = ((CoreLoader)(this.Owner)).savedLocation;

            tracker.captureAtLocation(targetLocation);

            // Hook up to the capture event, fired by the tracker.
            tracker.Capture += new SensorTracker.CaptureEventHandler(capture);
        }
    }
}
