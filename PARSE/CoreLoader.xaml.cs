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
using System.ComponentModel;

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
        public HistoryLoader               windowHistory;
        public MeasurementLoader           windowMeasurement;
        public DebugLoader                 windowDebug;

        //Modelling specific definitions
        private GeometryModel3D             Model;
        private GeometryModel3D             BaseModel;

        //New KinectInterpreter Class
        private KinectInterpreter           kinectInterp;
        private System.Threading.Timer      kinectCheck;

        //Image recognition specific definitions
        private bool                        capturedModel;
        private BitmapSource                modelimage;
        private bool                        capturedObject;
        private BitmapSource                objectimage;
        private int                         countdown;

        //point cloud lists for visualisation
        private List<PointCloud>            pcdl;
        private PointCloud                  pcd;

        //a stitcher
        private Stitcher                    stitcher; 

        //speech synthesizer instances
        private SpeechSynthesizer           ss;

        //prevents crashing on adjustment
        private Boolean kinectMovingLock = false;

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
            //define debug window
            windowDebug = new DebugLoader();
            windowDebug.Owner = this;
            
            windowDebug.sendMessageToOutput("Status", "Welcome to the PARSE Toolkit");
            windowDebug.sendMessageToOutput("Status", "Initializing Kinect Device");

            if (KinectSensor.KinectSensors.Count>0)
                {
                    windowDebug.sendMessageToOutput("Status", "Kinect found and online - " + KinectSensor.KinectSensors[0].DeviceConnectionId);
                }
                else
                {
                    windowDebug.sendMessageToOutput("Warning", "No Kinect Found");
                    //Check for kinect connection periodically
                    kinectCheck = new System.Threading.Timer(checkKinectConnection, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
                }
        }

        private void shutAnyWindows()
        {
            if (windowViewer != null)
            {
                windowViewer.Close();
            }
            if (windowScanner != null)
            {
                windowScanner.Close();
            }
            if (windowHistory != null)
            {
                windowHistory.Close();
            }
        }
        
        private void RGB_Click(object sender, RoutedEventArgs e)
        {
            this.shutAnyWindows();
            windowViewer = new ViewLoader("RGB");
            windowViewer.Owner = this;
            windowViewer.Show();
        }

        private void Depth_Click(object sender, RoutedEventArgs e)
        {
            this.shutAnyWindows(); 
            windowViewer = new ViewLoader("Depth");
            windowViewer.Owner = this;
            windowViewer.Show();
        }

        private void Skeleton_Click(object sender, RoutedEventArgs e)
        {
            this.shutAnyWindows();
            windowViewer = new ViewLoader("Skeleton");
            windowViewer.Owner = this;
            windowViewer.Show();
        }

        private void DepthIso_Click(object sender, RoutedEventArgs e)
        {
            this.shutAnyWindows();
            windowViewer = new ViewLoader("Depth Isolation");
            windowViewer.Owner = this;
            windowViewer.Show();
        }

        private void RGBIso_Click(object sender, RoutedEventArgs e)
        {
            this.shutAnyWindows();
            windowViewer = new ViewLoader("RGB Isolation");
            windowViewer.Owner = this;
            windowViewer.Show();
        }

        private void btnSensorUp_Click(object sender, RoutedEventArgs e)
        {
            if ((!kinectMovingLock) && (this.kinectInterp.kinectSensor.ElevationAngle + 5 <= this.kinectInterp.kinectSensor.MaxElevationAngle))
            {
                kinectMovingLock = true;
                this.kinectInterp.kinectSensor.ElevationAngle += 5;
            }
            kinectMovingLock = false;
        }

        private void btnSensorDown_Click(object sender, RoutedEventArgs e)
        {
            if ((!kinectMovingLock) && (this.kinectInterp.kinectSensor.ElevationAngle - 5) >= (this.kinectInterp.kinectSensor.MinElevationAngle))
            {
                kinectMovingLock = true;
                this.kinectInterp.kinectSensor.ElevationAngle -= 5;
            }
            kinectMovingLock = false;
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.kinectInterp.kinectReady)
            {
                Console.WriteLine("Stopping Kinect");
                this.kinectInterp.stopStreams();
                this.kinectInterp.kinectSensor.Stop();
            }
            Console.WriteLine("Main Window Closed - Exiting (0)");
            Environment.Exit(0);
        }

        private void NewScan_Click(object sender, RoutedEventArgs e)
        {
            this.shutAnyWindows();
            windowScanner = new ScanLoader();
            windowScanner.Owner = this;
            windowScanner.Show();
        }

        /* This will eventually form the recogniser *mechanism* for what ever
         * recognition will occur in the system. */
        private void surfTimer_tick(Object sender, EventArgs e)
        {

            if ((countdown > 0) && (!capturedModel))
            {
                countdown--;
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
            }
            else if ((countdown == 0) && (capturedModel))
            {
                capturedObject = true;
                objectimage = kinectInterp.getRGBTexture();
            }
            else if ((capturedModel) && (capturedObject))
            {
                //surfTimer.Stop();
            }

        }

        private void VolumeOption_Click(object sender, RoutedEventArgs e)
        {
            //define
            windowHistory = new HistoryLoader();
            windowHistory.Owner = this;

            //Static call to volume calculation method, pass persistent point cloud object
            PointCloud pc = pcd;
            Tuple<double, List<List<Point3D>>> T = VolumeCalculator.volume1stApprox(pc);
            List<List<Point3D>> planes = T.Item2;
            double volume = T.Item1;
            double height = HeightCalculator.getHeight(pc);
            windowHistory.runtimeTab.SelectedIndex = 0;
            windowHistory.visualisePlanes(planes, 1);
            windowHistory.voloutput.Content = volume + "m^3";
            windowHistory.heightoutput.Content = height + "m";

            List<double> areaList = AreaCalculator.getAllAreas(planes);

            windowHistory.areaList = areaList;

            //open Runtime viewer (aka results,history,output)
            windowHistory.Show();
        }

        private void LimbOption_Click(object sender, RoutedEventArgs e)
        {
            /*Requires generated model, raw depth array and previous*/
            windowScanner.determineLimbPlane(pcd);
            LimbCalculator.calculate(windowScanner.getYourMum(), windowScanner.getJointMeasurements());

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
            //Create .PCDs for use with the PCL Library to test stitching
            
            String filename = "";
            ICP.PointRGB[] point;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".PCD";
            dlg.Filter = "Point Cloud Data (.PCD)|*.PCD";

            if (dlg.ShowDialog() == true)
            {
                filename = dlg.FileName;   
            }

            for (int j = 0; j < pcdl.Count; j++)
            {
                //start subroutine to save to the PCD File's in a new directory
                TextWriter tw = new StreamWriter(filename+"_"+j);

                //write versioning info
                tw.WriteLine("# .PCD v1.6 - Point Cloud Data file format");
                tw.WriteLine("VERSION 1.6");

                //write metadata
                tw.WriteLine("FIELDS x y z rgb");
                tw.WriteLine("SIZE 4 4 4 4");
                tw.WriteLine("TYPE F F F F");
                tw.WriteLine("COUNT 1 1 1 1");
                tw.WriteLine("WIDTH " + pcdl[j].getAllPoints().Length);
                tw.WriteLine("HEIGHT 1");
                tw.WriteLine("VIEWPOINT 0 0 0 1 0 0 0");
                tw.WriteLine("POINTS " + pcdl[j].getAllPoints().Length);
                tw.WriteLine("DATA ascii");

                //store all points.
                //pc.rotate(new double[] { 0, 1, 0 }, -90);
                //pc.translate(new double[] { -1.5, 1.25, 0 });

                point = pcdl[j].getAllPoints();

                for (int i = 0; i < point.Length; i++)
                {
                    tw.WriteLine(point[i].point.X + " " + point[i].point.Y + " " + point[i].point.Z + " 4.2108e+06");
                }

                tw.Close();
            }

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
            
            windowViewer.Close();
            windowScanner = new ScanLoader(pcd);
            windowScanner.Owner = this;
            windowScanner.Show();

        }

        private void RemoveFeet_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < pcdl.Count; i++)
                {
                    pcdl[i].deleteFloor();
                }


            this.shutAnyWindows();
            windowScanner = new ScanLoader(pcdl);
            windowScanner.Owner = this;
            windowScanner.Show();
        }

        private void OpenDebug_Click(object sender, RoutedEventArgs e)
        {
            this.windowDebug.Show();
        }

        private void LoadScan_Click(object sender, RoutedEventArgs e)
        {
            /*Automates the following procedure:
             * 0) closes any viewer, opens runtime
             * 1) adds selected point cloud to visualiser
             * 2) groups it
             */

            /*0)*/
            this.shutAnyWindows();

     /*1)*/ Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".PARSE";
            dlg.Filter = "PARSE Reference Data (.PARSE)|*.PARSE";

            if (dlg.ShowDialog() == true)
            {
                String filename = dlg.FileName;
                this.DataContext = ScanSerializer.deserialize(filename);
                pcdl = ScanSerializer.depthPc;
            }

            System.Diagnostics.Debug.WriteLine("Performing end to end cloud processing...please wait.");

     /*2)*/ //instantiate the stitcher 
            stitcher = new BoundingBox();

            //jam points into stitcher
            stitcher.add(pcdl);
            stitcher.stitch();

            pcd = stitcher.getResult();
            pcdl = stitcher.getResultList();

            windowScanner = new ScanLoader(pcdl);
            windowScanner.Owner = this;
            windowScanner.Show();
        }

        private void AddMeasurement_Click(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count != 0)
            {
                this.shutAnyWindows();
                //Definition of window viewer seems to get lost somewhere
                this.OwnedWindows[0].Close();

                windowMeasurement = new MeasurementLoader();
                windowMeasurement.Owner = this;
                windowMeasurement.Show();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Please connect a Kinect Device");
            }
        }

        private void AddNewPatient_Click(object sender, RoutedEventArgs e)
        {
            //open patient detail viewer
            this.shutAnyWindows();
            windowPatient = new PatientLoader(true);
            windowPatient.Owner = this;
            windowPatient.Show();
        }

        private void checkKinectConnection(object state)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Kinect found and online - " + KinectSensor.KinectSensors[0].DeviceConnectionId);
            }
        }

        void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            //kinectInterp.stopStreams();
            //kinectInterp.kinectSensor.Stop();
            Environment.Exit(0);
        }
        
        
       
    }
}