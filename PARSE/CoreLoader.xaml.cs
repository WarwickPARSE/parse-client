//System imports
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Windows.Interop;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Xml.Serialization;
using HelixToolkit.Wpf;

//Kinect imports
using Microsoft.Kinect;
using PARSE.Recognition;
using PARSE.ICP;
using PARSE.ICP.Stitchers;

namespace PARSE
{
    /// <summary>
    /// Interaction logic for CoreLoader.xaml
    /// </summary>
    public partial class CoreLoader : Window
    {

        //UI definitions for child interfaces
        public ViewLoader                  windowViewer;
        public ScanLoader                  windowScanner;
        public Window                      windowPatient;
        public RuntimeLoader               windowRuntime;

        //Modelling specific definitions
        private GeometryModel3D             Model;
        private GeometryModel3D             BaseModel;

        //New KinectInterpreter Class
        private KinectInterpreter           kinectInterp;

        //Image recognition specific definitions
        private bool                        capturedModel;
        private BitmapSource                modelimage;
        private bool                        capturedObject;
        private BitmapSource                objectimage;
        private int                         countdown;

        //point cloud lists for visualisation
        private List<PointCloud>            fincloud;

        //a stitcher
        private Stitcher                    stitcher; 

        //speech synthesizer instances
        private SpeechSynthesizer           ss;

        private const double oneParseUnit = 2642.5;
        private const double oneParseUnitDelta = 7.5;
        //optimum distance for scanner

        public CoreLoader()
        {
            //Initialize Component
            InitializeComponent();

            //Initialize Database
            DatabaseEngine db = new DatabaseEngine();

            //Initialize KinectInterpreter
            kinectInterp = new KinectInterpreter(vpcanvas2);

            //ui initialization
            lblStatus.Content = kinectInterp.kinectStatus;
            this.WindowState = WindowState.Maximized;
            ss = new SpeechSynthesizer();

            //Miscellaneous modelling definitions
            Model = new GeometryModel3D();
            BaseModel = new GeometryModel3D();

        }

        /// <summary>
        /// WPF Form Methods
        /// </summary>
        /// <param name="sender">originator of event</param>
        /// <param name="e">event identifier</param>

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Open child windows

            try
            {
                //open default window viewer
                windowViewer = new ViewLoader();
                windowViewer.Owner = this;
                windowViewer.Show();

                //open patient detail viewer
                windowPatient = new PatientLoader();
                windowPatient.Owner = this;
                windowPatient.Show();

                //open runtime detail viewer
                windowRuntime = new RuntimeLoader();
                windowRuntime.Owner = this;
                windowRuntime.Show();

                windowRuntime.sendMessageToOutput("Status", "Welcome to the PARSE Toolkit");
                windowRuntime.sendMessageToOutput("Status", "Initializing Kinect Device");
                ss.Speak("Welcome!");

                if (KinectSensor.KinectSensors.Count>0)
                {
                    windowRuntime.sendMessageToOutput("Status", "Kinect found and online - " + KinectSensor.KinectSensors[0].DeviceConnectionId);
                }
                else
                {
                    windowRuntime.sendMessageToOutput("Warning", "No Kinect Found");
                }

                //initialize scanner detail viewer
                windowScanner = new ScanLoader();
                windowScanner.Owner = this;
                windowScanner.Closed += new EventHandler(windowScanner_Closed);

            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
            }
        }

        //TODO: prevent the following two methods from crashing if called in quick succession
        private void btnSensorUp_Click(object sender, RoutedEventArgs e)
        {
            if (this.kinectInterp.kinectSensor.ElevationAngle != this.kinectInterp.kinectSensor.MaxElevationAngle)
            {
                this.kinectInterp.kinectSensor.ElevationAngle += 5;
            }
        }

        private void btnSensorDown_Click(object sender, RoutedEventArgs e)
        {
            if (this.kinectInterp.kinectSensor.ElevationAngle != this.kinectInterp.kinectSensor.MinElevationAngle)
            {
                this.kinectInterp.kinectSensor.ElevationAngle -= 5;
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (this.kinectInterp.kinectReady)
            {
                this.kinectInterp.kinectSensor.Stop();
            }

        }

        private void NewScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                windowScanner = new ScanLoader();
                windowScanner.Owner = this;
                windowScanner.Closed += new EventHandler(windowScanner_Closed);
                windowScanner.Show();
                windowViewer.Close();
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
            }  
        }

        void windowScanner_Closed(object sender, EventArgs e)
        {
            windowViewer = new ViewLoader();
            windowViewer.Owner = this;
            windowViewer.Show();
        }


        /* This will eventually form the recogniser *mechanism* for what ever
         * recognition will occur in the system. */

        private void surfTimer_tick(Object sender, EventArgs e)
        {

            if ((countdown > 0) && (!capturedModel))
            {
                countdown--;
                label4.Content = "Present scanner to camera...." + countdown.ToString() + "...";
            }
            else if ((countdown == 0) && (!capturedModel))
            {
                capturedModel = true;
                modelimage = kinectInterp.getRGBTexture();
                countdown = 10;
            }
            else if ((countdown > 0) && (capturedModel))
            {
                countdown--;
                label4.Content = "Place scanner on patient..." + countdown.ToString() + "...";
            }
            else if ((countdown == 0) && (capturedModel))
            {
                capturedObject = true;
                objectimage = kinectInterp.getRGBTexture();
                label4.Content = "Looking for scanner..";
            }
            else if ((capturedModel) && (capturedObject))
            {
                //surfTimer.Stop();
            }

        }

        private void VolumeOption_Click(object sender, RoutedEventArgs e)
        {
            //Static call to volume calculation method, pass persistent point cloud object
            VolumeCalculator.calculateVolume(windowScanner.getYourMum());
            ss.Speak("You are fat");
            System.Windows.Forms.MessageBox.Show("You are too fat");
        }

        private void LimbOption_Click(object sender, RoutedEventArgs e)
        {
            //open windowviewer with isolation method.
            windowScanner.Closed += new EventHandler(windowScanner_Closed);
            
            /*Requires generated model, raw depth array and previous*/
            //windowViewer.setLimbVisualisation();
            LimbCalculator.calculate(windowScanner.getPointClouds()[0], windowScanner.getJointMeasurements());

        }

        private void ImportScan_Click(object sender, RoutedEventArgs e)
        {

            /*Import scan currently imports files based on the assumption that they
             * have been serialized as a visualisation object. Once the point cloud 
             * class has been been implemented, it will assume that it is dealing
             * with point cloud objects which will then be passed to the visualisation
             * method as appropriate */

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".PARSE";
            dlg.Filter = "PARSE Reference Data (.PARSE)|*.PARSE";

            if (dlg.ShowDialog() == true)
            {
                String filename = dlg.FileName;
                this.DataContext = ScanSerializer.deserialize(filename);
                fincloud = ScanSerializer.depthPc;
            }

            try
            {
                windowScanner = new ScanLoader(fincloud);
                windowScanner.Owner = this;
                windowScanner.Closed += new EventHandler(windowScanner_Closed);
                windowScanner.Show();
                windowViewer.Close();
            }

            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
            }  

        }

        private void ExportScan_Click(object sender, RoutedEventArgs e)
        {

            /*ExportScan serializes the visualisation object, once the pointcloud
             * structure has been implemented, it will serialize the pc object
             * rather than this visualisation. Current issue, cant serialize list
             objects that contain arrays apparantely.*/

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".PARSE";
            dlg.Filter = "PARSE Reference Data (.PARSE)|*.PARSE";

            if (dlg.ShowDialog() == true)
            {
                String filename = dlg.FileName;
                ScanSerializer.serialize(filename, windowScanner.getPointClouds());
            }

        }

        private void ExportScanPCD_Click(object sender, RoutedEventArgs e)
        {
            //Create .PCD for use with the PCL Library
            PointCloud pc = windowScanner.getPointClouds()[0];
            String filename = "";

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".PCD";
            dlg.Filter = "Point Cloud Data (.PCD)|*.PCD";

            if (dlg.ShowDialog() == true)
            {
                filename = dlg.FileName;
            }

            //start subroutine to save to PCD File
            TextWriter tw = new StreamWriter(filename);
            ICP.PointRGB[] point;

            //write versioning info
            tw.WriteLine("# .PCD v1.6 - Point Cloud Data file format");
            tw.WriteLine("VERSION 1.6");

            //write metadata
            tw.WriteLine("FIELDS x y z rgb");
            tw.WriteLine("SIZE 4 4 4 4");
            tw.WriteLine("TYPE F F F F");
            tw.WriteLine("COUNT 1 1 1 1");
            tw.WriteLine("WIDTH " + pc.getAllPoints().Length);
            tw.WriteLine("HEIGHT 1");
            tw.WriteLine("VIEWPOINT 0 0 0 1 0 0 0");
            tw.WriteLine("POINTS " + pc.getAllPoints().Length);
            tw.WriteLine("DATA ascii");

            //store all points.
            //pc.rotate(new double[] { 0, 1, 0 }, -90);
            //pc.translate(new double[] { -1.5, 1.25, 0 });
            point = pc.getAllPoints();

            for(int i = 0; i < point.Length; i++) {
                tw.WriteLine(point[i].point.X + " " + point[i].point.Z + " " + point[i].point.Y + " 4.2108e+06");
            }

            tw.Close();

        }

        private void SimpleStitchTest_Click(object sender, RoutedEventArgs e)
        {
            List<PointCloud> pc = windowScanner.getPointClouds();
            PointCloud pcd= new PointCloud();

            //instantiate the stitcher 
            stitcher = new BoundingBox();
            
            //jam points into stitcher
            stitcher.add(pc);
            stitcher.stitch();
            
            pcd = stitcher.getResult(); 
            
            windowScanner.Close();
            windowViewer.Close();
            windowScanner = new ScanLoader(pcd);
            windowScanner.Owner = this;
            windowScanner.Closed += new EventHandler(windowScanner_Closed);
            windowScanner.Show();

        }
       
    }
}