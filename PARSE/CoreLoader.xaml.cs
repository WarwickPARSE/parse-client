//System imports
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Xml.Serialization;
using HelixToolkit.Wpf;

//Kinect imports
using Microsoft.Kinect;
using PARSE.ICP;
using PARSE.ICP.Stitchers;
using PARSE.Recognition;

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

        //point cloud lists for visualisation
        private List<PointCloud>            pcdl;
        private PointCloud                  pcd;

        //a stitcher
        private Stitcher                    stitcher; 

        //speech synthesizer instances
        private SpeechSynthesizer           sandra;

        //prevents crashing on adjustment
        private Boolean kinectMovingLock = false;

        //database engine
        DatabaseEngine db;

        private const double oneParseUnit = 2642.5;
        private const double oneParseUnitDelta = 7.5;

        public CoreLoader()
        {
            //Initialize Component
            InitializeComponent();

            //Initialize Database
            db = new DatabaseEngine();

            //Initialize KinectInterpreter
            kinectInterp = new KinectInterpreter(vpcanvas2);
            this.kinectmenu.IsEnabled = false;
            this.newscan.IsEnabled = false;

            //ui initialization
            this.WindowState = WindowState.Maximized;
            sandra = new SpeechSynthesizer();

            //Miscellaneous modelling definitions
            Model = new GeometryModel3D();
            BaseModel = new GeometryModel3D();

            this.resetButtons();

        }

        /// <summary>
        /// window_loaded
        /// </summary>
        /// <param name="sender">originator of event</param>
        /// <param name="e">event identifier</param>
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //define debug window
            windowDebug = new DebugLoader();
            windowDebug.Owner = this;
            
            //output to debug window
            windowDebug.sendMessageToOutput("Status", "Welcome to the PARSE Toolkit");
            windowDebug.sendMessageToOutput("Status", "Initializing Kinect Device");
            
            //check kinect sensors
            if (KinectSensor.KinectSensors.Count>0)
                {
                    windowDebug.sendMessageToOutput("Status", "Kinect found and online - " + KinectSensor.KinectSensors[0].DeviceConnectionId);
                    this.newscan.IsEnabled = true;
                    this.kinectmenu.IsEnabled = true;
                }
                else
                {
                    windowDebug.sendMessageToOutput("Warning", "No Kinect Found");
                    //Check for kinect connection periodically
                    kinectCheck = new System.Threading.Timer(checkKinectConnection, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
                }

            //get version number
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            verno.Content = "Version no: " + version;

            db.con.Close();

        }

        public void setPC(PointCloud pc, List<PointCloud> pcl)
        {
            this.pcd = pc;
            this.pcdl = pcl;
        }

        private Boolean noSensor()
        {
            if (this.kinectInterp.noKinect())
            {
                sandra.Speak("No Kinect Detected");
                this.kinectmenu.IsEnabled = false;
                this.newscan.IsEnabled = false;
                kinectCheck = new System.Threading.Timer(checkKinectConnection, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
                return true;
            }
            return false;
            
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
        }
        
        private void RGB_Click(object sender, RoutedEventArgs e)
        {
            if (this.noSensor())
            {
                return;
            }
            this.shutAnyWindows();
            windowViewer = new ViewLoader("RGB");
            windowViewer.Owner = this;
            windowViewer.Show();
        }

        private void Depth_Click(object sender, RoutedEventArgs e)
        {
            if (this.noSensor())
            {
                return;
            }
            this.shutAnyWindows(); 
            windowViewer = new ViewLoader("Depth");
            windowViewer.Owner = this;
            windowViewer.Show();
        }

        private void Skeleton_Click(object sender, RoutedEventArgs e)
        {
            if (this.noSensor())
            {
                return;
            }
            this.shutAnyWindows();
            windowViewer = new ViewLoader("Skeleton");
            windowViewer.Owner = this;
            windowViewer.Show();
        }

        private void DepthIso_Click(object sender, RoutedEventArgs e)
        {
            if (this.kinectInterp.noKinect())
            {
                return;
            }
            this.shutAnyWindows();
            windowViewer = new ViewLoader("Depth Isolation");
            windowViewer.Owner = this;
            windowViewer.Show();
        }

        private void RGBIso_Click(object sender, RoutedEventArgs e)
        {
            if (this.noSensor())
            {
                return;
            }
            this.shutAnyWindows();
            windowViewer = new ViewLoader("RGB Isolation");
            windowViewer.Owner = this;
            windowViewer.Show();
        }

        private void btnSensorUp_Click(object sender, RoutedEventArgs e)
        {
            if (this.noSensor())
            {
                return;
            }
            if ((!kinectMovingLock) && (this.kinectInterp.kinectSensor.ElevationAngle + 5 <= this.kinectInterp.kinectSensor.MaxElevationAngle))
            {
                kinectMovingLock = true;
                this.kinectInterp.kinectSensor.ElevationAngle += 5;
            }
            kinectMovingLock = false;
        }

        private void btnSensorDown_Click(object sender, RoutedEventArgs e)
        {
            if (this.noSensor())
            {
                return;
            }
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

        private void resetButtons()
        {
            this.export1.IsEnabled = false;
            this.export2.IsEnabled = false;
            this.measurement.IsEnabled = false;
            this.removefloor.IsEnabled = false;
            //this.calculate.IsEnabled = false;
        }
        
        private void NewScan_Click(object sender, RoutedEventArgs e)
        {
            if (this.noSensor())
            {
                return;
            }
            
            this.shutAnyWindows();

            this.resetButtons();
            
            windowScanner = new ScanLoader();
            windowScanner.Owner = this;
            windowScanner.Show();

            windowHistory = new HistoryLoader();
            windowHistory.Owner = this;
        }

        private void VolumeOption_Click(object sender, RoutedEventArgs e)
        {
            //Static call to volume calculation method, pass persistent point cloud object
            Tuple<List<List<Point3D>>, double> T = PlanePuller.pullAll(pcd);
            
            List<List<Point3D>> planes = T.Item1;
            double increment = T.Item2;
            
            double volume = VolumeCalculator.volume1stApprox(planes,increment);
            volume = Math.Round(volume,4);
            sandra.Speak("Your Volume is " + Math.Round(volume / 0.058, 2) + " Bernards!");

            List<double> areaList = AreaCalculator.getAllAreas(planes);
            windowHistory.areaList = areaList;
            
            windowHistory.runtimeTab.SelectedIndex = 0;
            windowHistory.visualisePlanes(planes, 1);
            windowHistory.voloutput.Content = volume + "m\u00B3";
            
            //show Runtime viewer (aka results,history)
            windowHistory.Show();
        }

        private void LimbOption_Click(object sender, RoutedEventArgs e)
        {

            /*gets all the planes by calling volume calculator*/
            if (pcd != null)
            {
                Tuple<List<List<Point3D>>, double> T = PlanePuller.pullAll(pcd);
                List<List<Point3D>> planes = T.Item1;
                /*Requires generated model, raw depth array and previous*/
                windowScanner.determineLimb(pcd);
            }
            else
            {
                HistoryLoader windowHistory = new HistoryLoader();
                windowHistory.Owner = this;
                windowHistory.Show();
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
                this.export1.IsEnabled = false;
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
                this.export2.IsEnabled = false;
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
            List<PointCloud> pc = pcdl;
            pcd = new PointCloud();

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

            this.calculate.IsEnabled = true;
            this.measurement.IsEnabled = true;

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
             * 0) kills kinect, closes any viewer, resets buttons
             * 1) adds selected point cloud to visualiser
             * 2) groups it
             * 3) calcs height
             */

            try
            {
                /*0)*/
                //this.kinectInterp.stopStreams();
                this.shutAnyWindows();
                this.resetButtons();

                /*1)*/
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = ".PARSE";
                dlg.Filter = "PARSE Reference Data (.PARSE)|*.PARSE";

                String filename = "";

                if (dlg.ShowDialog() == true)
                {
                    filename = dlg.FileName;
                }

                if ((filename == null) || (dlg.FileName.Length == 0))
                {

                    return;
                }

                // Show the window first - keep UI speedy!
                System.Diagnostics.Debug.WriteLine("Showing window");
                windowScanner = new ScanLoader();
                windowScanner.Owner = this;

                // Do UI stuff on UI thread
                this.export1.IsEnabled = false;
                this.export2.IsEnabled = false;
                this.removefloor.IsEnabled = true;

                //define
                windowHistory = new HistoryLoader();
                windowHistory.Owner = this;

                // Background thread to get all the heavy computation off of the UI thread
                BackgroundWorker B = new BackgroundWorker();
                B.DoWork += new DoWorkEventHandler(loadScanThread);

                // Catch the progress update events
                B.WorkerReportsProgress = true;
                B.ProgressChanged += new ProgressChangedEventHandler((obj, args) =>
                    {
                        windowScanner.loadingwidgetcontrol.UpdateProgressBy(args.ProgressPercentage);
                        if (args.UserState != null)
                        {
                            if (args.UserState is string)
                            {
                                System.Diagnostics.Debug.WriteLine((string)args.UserState);
                            }
                            else if (args.UserState is Action)
                            {
                                ((Action)args.UserState)();
                            }
                        }
                    });
                B.RunWorkerCompleted += new RunWorkerCompletedEventHandler((obj, args) =>
                {
                    windowScanner.processCloudList(pcdl, windowScanner.loadingwidgetcontrol);
                });

                // GOOO!!! Pass the file name so it can be loaded
                B.RunWorkerAsync(filename);
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err.ToString());
            }

        }

        private void loadScanThread(Object sender, DoWorkEventArgs e)
        {
            // Cast object back into BackgroundWorker
            BackgroundWorker B = (BackgroundWorker)sender;
            B.ReportProgress(1, "Background worker running");

            ScanSerializer.deserialize((string)e.Argument);
            B.ReportProgress(8, "Model deserialised");
            pcdl = ScanSerializer.depthPc;
            B.ReportProgress(2, "Model loaded");

            /*2)*/
            //instantiate the stitcher 
                stitcher = new BoundingBox();
            B.ReportProgress(1);

                //jam points into stitcher
                stitcher.add(pcdl);
            B.ReportProgress(1);
            
                stitcher.stitch();
            B.ReportProgress(5);
            
                pcd = stitcher.getResult();
                pcdl = stitcher.getResultList();
            B.ReportProgress(1, "Point Cloud Stitched");

            // Get the height
            double height = Math.Round(HeightCalculator.getHeight(pcd), 3);
            Dispatcher.BeginInvoke((Action)(() => { windowHistory.heightoutput.Content = height + "m"; }));
            B.ReportProgress(1);

            //this.kinectInterp.stopStreams();
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
            windowPatient = new PatientLoader(false);
            windowPatient.Owner = this;
            windowPatient.Show();
            ApplyBlur(this);
            windowPatient.Closed += new EventHandler(RemoveBlur_Closed);
        }

        private void checkKinectConnection(object state)
        {
            Action method = () => this.hasKinectBeenAdded();
            this.Dispatcher.Invoke(method);  
        }

        private Boolean hasKinectBeenAdded()
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Kinect found and online - " + KinectSensor.KinectSensors[0].DeviceConnectionId);
                this.kinectInterp.setSensor(KinectSensor.KinectSensors[0]);
                this.kinectmenu.IsEnabled = true;
                this.newscan.IsEnabled = true;
                sandra.Speak("Kinect Detected");
                kinectCheck.Dispose();
                return true;

            }
            return false;
        }

        private void ApplyBlur(Window window)
        {
            //applies blur to windows not in the foreground as and when appropriate.

            System.Windows.Media.Effects.BlurEffect windowBlur = new System.Windows.Media.Effects.BlurEffect();
            windowBlur.Radius = 4;
            window.Effect = windowBlur;

        }

        private void RemoveBlur_Closed(object sender, EventArgs e)
        {

            this.Effect = null;

        }

        void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            //kinectInterp.stopStreams();
            //kinectInterp.kinectSensor.Stop();
            Environment.Exit(0);
        }
        
        
       
    }
}