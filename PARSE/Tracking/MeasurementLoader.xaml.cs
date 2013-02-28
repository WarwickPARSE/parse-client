using System;
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
        
        private System.Windows.Forms.Timer pcTimer;
        private int countdown;

        //Kinect instance
        private KinectInterpreter kinectInterp;

        // Skeleton
        private Dictionary<JointType, double[]> jointDepths;

        // speech synthesizer instances
        private SpeechSynthesizer ss;

        private Canvas tmpCanvas;

        SensorTracker tracker;


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

            //start scanning procedure
            //kinectInterp = new KinectInterpreter(skeloutline);
            //kinectInterp.stopStreams();
            if (KinectSensor.KinectSensors.Count > 0)
                System.Diagnostics.Debug.WriteLine("Measurement Window Ready");
            else
                System.Diagnostics.Debug.WriteLine("Measurement Window Ready, but no Kinect detected");
        }

        private void cancel_scan_Click(object sender, RoutedEventArgs e)
        {
            tracker.Stop();
            //this.Close();
        }

        private void start_scan_Click(object sender, RoutedEventArgs e)
        {
            // hide buttons from form
            //cancel_scan.Visibility = Visibility.Collapsed;
            start_scan.Visibility = Visibility.Collapsed;
            //instructionblock.Visibility = Visibility.Collapsed;
            instructionblock.Text = "-";
            // show image
            Visualisation.Visibility = Visibility.Visible;

            System.Diagnostics.Debug.WriteLine("Starting measurement visualisation");

            // Start tracking
            tracker = new SensorTracker(Visualisation, this, true, instructionblock);
            tracker.Start();
        }

        /*Publicly accessible methods*/

        public Dictionary<JointType, double[]> enumerateSkeletonDepths(Skeleton sk)
        {
            //Store double
            Dictionary<JointType, double[]> jointDepths = new Dictionary<JointType, double[]>();

            //Get depths and x,y locations at joints.
            foreach (Joint j in sk.Joints)
            {
                double[] positions = { j.Position.Z, j.Position.X, j.Position.Y };
                jointDepths.Add(j.JointType, positions);
            }

            return jointDepths;
        }

        public Dictionary<JointType, double[]> getJointMeasurements()
        {
            return jointDepths;
        }

        /* Setters */
        public void setLimbCues(bool measureMode)
        {
            if (measureMode)
            {
                this.instructionblock.Visibility = Visibility.Visible;
                skeloutline.Visibility = Visibility.Visible;
                this.instructionblock.Text = "Click on an area of the body";
            }
        }

        internal void capture(int x, int y, double angleXY, double angleZ)
        {
            System.Diagnostics.Debug.WriteLine("Scan captured! END");
            tracker.Stop();
            this.Close();
        }
    }
}
