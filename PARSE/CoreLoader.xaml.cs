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
using System.Windows.Controls.DataVisualization.Charting;

//Kinect imports
using Microsoft.Kinect;
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
        public PatientLoader               windowPatient;
        public HistoryLoader               windowHistory;
        public MeasurementLoader           windowMeasurement;
        public DebugLoader                 windowDebug;
        public MetaLoader                  windowMeta;

        //Window setup states
        public enum OperationModes
        {
            ShowExistingCloud, ShowExistingPatient, ShowExistingResults,
            CaptureNewCloud, CaptureNewPatient
        };

        //Modelling specific definitions
        private GeometryModel3D             Model;
        private GeometryModel3D             BaseModel;

        //New KinectInterpreter Class
        private KinectInterpreter           kinectInterp;
        private System.Threading.Timer      kinectCheck;

        //point cloud lists for visualisation
        private List<PointCloud>            pcdl;
        private PointCloud                  pcd;
        private String                      filename;

        //a stitcher
        private Stitcher                    stitcher; 

        //speech synthesizer instances
        private SpeechSynthesizer           sandra;

        //prevents crashing on adjustment
        private Boolean kinectMovingLock = false;

        //database engine
        DatabaseEngine db;

        //current working directory for parse files
        private String                      workingDir;

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

        }

        /// <summary>
        /// sets values for pointcloud (stitching) and point cloud list (visualisation)
        /// </summary>
        /// <param name="pc">a point cloud</param>
        /// <param name="pcl">a list of point clouds</param>

        public void setPC(PointCloud pc, List<PointCloud> pcl)
        {
            this.pcd = pc;
            this.pcdl = pcl;
            filename = null;
        }

        /// <summary>
        /// Returns true iff there is no Kinect connected. If this method returns true, it will spawn a thread to check if a sensor has been connected, polls every 10 seconds
        /// </summary>
        /// <returns>Boolean</returns>
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
        
        /// <summary>
        /// Shuts any windows that do not need to be open
        /// </summary>
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
        
        /// <summary>
        /// opens rgb feed for viewing
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

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

        /// <summary>
        /// opens depth feed for viewing
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

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

        /// <summary>
        /// opens skeleton feed for viewing
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

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

        /// <summary>
        /// opens the depth isolation feed
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

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

        /// <summary>
        /// opens the rgb isolation feed
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

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

        /// <summary>
        /// moves the sensor up 5 degrees
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

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

        /// <summary>
        /// moves the sensor down 5 degrees
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

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

        /// <summary>
        /// runs clean up upon window closing, stops kinect streams
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

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

        /// <summary>
        /// enables particular buttons for measurement and calculation
        /// </summary>

        private void resetButtons()
        {
            this.export1.IsEnabled = false;
            this.export2.IsEnabled = false;
            this.measurement.IsEnabled = true;
            this.removefloor.IsEnabled = false;
            this.calculate.IsEnabled = false;
        }

        /// <summary>
        /// displays a new scan loader screen 
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>
        
        private void NewScan_Click(object sender, RoutedEventArgs e)
        {
            if (this.noSensor())
            {
                return;
            }
            
            this.shutAnyWindows();

            this.resetButtons();
            
            windowScanner = new ScanLoader((int)ScanLoader.OperationModes.CaptureNewCloud);
            windowScanner.Owner = this;
            windowScanner.Show();

            windowHistory = new HistoryLoader();
            windowHistory.Owner = this;
        }

        /// <summary>
        /// runs the volume calculation subroutine on an open point cloud/patient
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

        private void VolumeOption_Click(object sender, RoutedEventArgs e)
        {
            //Static call to volume calculation method, pass persistent point cloud object
            Tuple<List<List<Point3D>>, double> T = PlanePuller.pullAll(pcd);
            
            List<List<Point3D>> planes = T.Item1;
            double increment = T.Item2;
            
            double volume = VolumeCalculator.volume1stApprox(planes,increment);
            volume = Math.Round(volume,4);

            List<double> areaList = AreaCalculator.getAllAreas(planes);
            windowHistory.areaList = areaList;
            
            windowHistory.runtimeTab.SelectedIndex = 0;
            windowHistory.visualisePlanes(planes, 1);
            windowHistory.voloutput.Content = volume + "m\u00B3";
            windowHistory.heightoutput.Content = HeightCalculator.getHeight(pcd) + "m";
            windowHistory.scantime.Content = "Weight (Est): " + VolumeCalculator.calculateApproxWeight(volume) + "kg";
            windowHistory.scanfileref.Content = "BMI Measure: " + VolumeCalculator.calculateBMI(VolumeCalculator.calculateApproxWeight(volume),HeightCalculator.getHeight(pcd));
            windowHistory.scanvoxel.Content = "Siri (%BF): " + VolumeCalculator.calculateSiri(volume, VolumeCalculator.calculateApproxWeight(volume), HeightCalculator.getHeight(pcd)) + "%";
            
            //show Runtime viewer (aka results,history)
            windowHistory.Show();

            List<Tuple<DateTime, double>> records = this.getTimeStampsAndVals((int) Convert.ToInt64(windowPatient.patientIDExisting.Content));

            int historyLookBack = 5;

            if ((records != null) && (records.Count > 0))
            {
                int size = Math.Min(records.Count, historyLookBack);

                KeyValuePair<DateTime, double>[] records2 = new KeyValuePair<DateTime, double>[size];

                for (int i = 0; i < size; i++)
                {
                    records2[i] = new KeyValuePair<DateTime, double>(records[i].Item1, records[i].Item2);
                }

                //set change in volume... may need refinement
                if (size != 0)
                {
                    double change = 0;
                    change = (volume - records[records.Count - 1].Item2)/records[records.Count - 1].Item2;//may need to become records[records.Count-2].Item2 later
                    windowHistory.volchangeoutput.Content = Math.Round(100 * change, 2) + "%";
                }
                else
                {
                    windowHistory.volchangeoutput.Content = "Not Enough Info";
                    windowHistory.volchart.Visibility = Visibility.Collapsed;
                }
                //setData
                ((LineSeries)(windowHistory.volchart.Series[0])).ItemsSource = records2;
            }
            else
            {
                windowHistory.volchangeoutput.Content = "Not Enough Info";
                windowHistory.volchart.Visibility = Visibility.Collapsed;
            }

            System.Media.SoundPlayer player = new System.Media.SoundPlayer();
            player.SoundLocation = "Base.wav";
            player.Play();
            
        }

        /// <summary>
        /// calculates all limbs as defined by limb calculator
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

        private void LimbOption_Click(object sender, RoutedEventArgs e)
        {
            /*Create an array of type tuple<double,double,List<List<Point3D>>>
             * as limbs will all be calculated before displaying history loader
             * results, not partic. efficient but fine given the restriction.*/

            //gets all the planes by calling volume calculator

            //kinect sensor check is here, can't use coord mapper otherwise.

            if (KinectSensor.KinectSensors.Count > 0)
            {
                Tuple<List<List<Point3D>>, double> T = PlanePuller.pullAll(pcd);
                List<List<Point3D>> planes = T.Item1;
                /*Requires generated model, raw depth array and previous*/
                List<Tuple<double,double,List<List<Point3D>>>> result = windowScanner.determineLimb(pcd);
                /*Then open history loader (limb circum stuff will be set here soon)*/
                HistoryLoader windowHistory = new HistoryLoader();
                windowHistory.runtimeTab.SelectedIndex = 1;
                windowHistory.Owner = this;
                windowHistory.Show();
                windowHistory.visualiseLimbs(result, 1, 1);

            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(this, "You need a Kinect to perform this action.","Kinect Sensor Missing", MessageBoxButton.OK, MessageBoxImage.Stop);
            }

        }

        /// <summary>
        /// exports a scan into the infamous .PARSE format
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

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

        /// <summary>
        /// exports a scan into the not so infamous PCD format for debugging with PCL
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

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

        /// <summary>
        /// performs a simple stitch test (deprecated? robin?)
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

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

        /// <summary>
        /// fine tunes the point cloud (just removes floor)
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

        private void RemoveFeet_Click(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < pcdl.Count; i++)
                {
                    pcdl[i].deleteFloor();
                }

            this.calculate.IsEnabled = true;
            this.measurement.IsEnabled = true;

            this.shutAnyWindows();

            windowScanner = new ScanLoader((int)ScanLoader.OperationModes.ShowExistingResults);
            windowScanner.Owner = this;

            // Do UI stuff on UI thread
            this.export1.IsEnabled = false;
            this.export2.IsEnabled = false;
            this.removefloor.IsEnabled = true;

            //define
            windowHistory = new HistoryLoader();
            windowHistory.Owner = this;

            // Background thread to get all the heavy computation off of the UI thread
            /*
            BackgroundWorker B = new BackgroundWorker();
             */
            //TODO Confirm we don't need this - the point cloud is loaded and stitched already?
            //B.DoWork += new DoWorkEventHandler(loadScanThread);
            /*
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
            {*/
                windowScanner.processCloudList(pcdl, windowScanner.loadingwidgetcontrol);
            /*
            });


            // GOOO!!! Pass the file name so it can be loaded
            B.RunWorkerAsync(null);
            */
        }

        /// <summary>
        /// opens the debug window for logging purposes
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

        private void OpenDebug_Click(object sender, RoutedEventArgs e)
        {
            this.windowDebug.Show();
        }

        /// <summary>
        /// opens the load scan metaloader to few current patients in the database
        /// </summary>
        /// <param name="sender">the object</param>
        /// <param name="e">the routed event</param>

        private void LoadScan_Click(object sender, RoutedEventArgs e)
        {

            //Load metaloader with list of currently recorded patients provide the option to just load point cloud if required.

            windowMeta = new MetaLoader();
            windowMeta.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            windowMeta.Owner = this;
            windowMeta.button3.Visibility = Visibility.Collapsed;
            windowMeta.Show();

            windowMeta.Closing += new CancelEventHandler(windowMeta_Closing);

        }

        /// <summary>
        /// opens new record with selected patient in patient loader
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void windowMeta_Closing(object sender, CancelEventArgs e)
        {
            //This is called when window meta is closed as control needs to be passed back to coreloader
            //to load the relevant windows based on what scans a particular patient may have undergone.

            Tuple<int,String,String> activeRecord = windowMeta.returnSelectedRecord();

            //Load patient loader with new patient information using the existing constructor.

            if (activeRecord != null)
            {
                windowPatient = new PatientLoader(activeRecord.Item1, (int)OperationModes.ShowExistingResults);
                windowPatient.Owner = this;
                windowPatient.Show();
            }

        }

        /// <summary>
        /// performs scan loading on a thread, child method of load scan.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void loadScanThread(Object sender, DoWorkEventArgs e)
        {
            // Cast object back into BackgroundWorker

            BackgroundWorker B = (BackgroundWorker)sender;
            B.ReportProgress(1, "Background worker running");

            String filename = (string)e.Argument;

            if (filename != null)
            {
                B.ReportProgress(0, "Loading file: " + filename);

                ScanSerializer.deserialize(filename);

                System.Diagnostics.Debug.WriteLine(e.Argument);

                B.ReportProgress(8, "Model deserialised");

                pcdl = ScanSerializer.depthPc;

                System.Diagnostics.Debug.WriteLine(pcdl.Count);

                B.ReportProgress(2, "Model loaded");
            }

            if (pcdl.Count == 0)
                throw new PointCloudException("PCDL is empty");

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
            B.ReportProgress(1, "Point Cloud Stitched (with " + pcdl.Count + " components)");
            if (pcdl.Count == 0)
                throw new PointCloudException("Stitcher returned empty point cloud list");

            // Get the height
            double height = Math.Round(HeightCalculator.getHeight(pcd), 3);
            Dispatcher.BeginInvoke((Action)(() => { windowHistory.heightoutput.Content = height + "m"; }));
            B.ReportProgress(1);
        }

        /// <summary>
        /// calls the markerless recognition methods with add measurement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

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

        /// <summary>
        /// calls patient loader with blank fields for entering a new patient
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void AddNewPatient_Click(object sender, RoutedEventArgs e)
        {
            //open patient detail viewer
            this.shutAnyWindows();
            windowPatient = new PatientLoader(false);
            windowPatient.Owner = this;
            windowPatient.Show();
        }

        /// <summary>
        /// checks if the kinect connection
        /// </summary>
        /// <param name="state">the state</param>

        private void checkKinectConnection(object state)
        {
            Action method = () => this.hasKinectBeenAdded();
            this.Dispatcher.Invoke(method);  
        }

        /// <summary>
        /// checks if kinect has been added to the sensor list, called by dispatcher checkKinectConnection(state)
        /// </summary>
        /// <returns>true if there is a kinect, false otherwise</returns>

        private Boolean hasKinectBeenAdded()
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Kinect found and online - " + KinectSensor.KinectSensors[0].DeviceConnectionId);
                this.kinectInterp.setSensor(KinectSensor.KinectSensors[0]);
                this.kinectmenu.IsEnabled = true;
                this.newscan.IsEnabled = true;
                this.measurement.IsEnabled = true;
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

        private void RemoveBlur()
        {

            this.Effect = null;

        }

        void MenuItem_Exit(object sender, RoutedEventArgs e)
        {
            //kinectInterp.stopStreams();
            //kinectInterp.kinectSensor.Stop();
            Environment.Exit(0);
        }

        private void OpenPatient_Click(object sender, RoutedEventArgs e)
        {
            this.LoadPointCloudFromFile();
        }

        /// <summary>
        /// Loads a pointcloud from a file, if patientid=0 then assume no database record
        /// </summary>
        /// <param name="patientid">patient id, 0 if just loading point cloud normally</param>

        public void LoadPointCloudFromFile(int patientid=0)
        {
            
            LinkedListNode<int> scanID;
            LinkedListNode<DateTime> timestamp;
            DateTime latestScanTime = new DateTime();
            int latestScanTimeID = 0;

            try 
            {

                if (patientid.Equals(0))
                {
                    //If patient does not exist in database, just load their point cloud
                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    dlg.DefaultExt = ".PARSE";
                    dlg.Filter = "PARSE Reference Data (.PARSE)|*.PARSE";

                    filename = "";

                    if (dlg.ShowDialog() == true)
                    {
                        filename = dlg.FileName;
                        this.shutAnyWindows();
                        this.resetButtons();
                    }

                    if ((filename == null) || (dlg.FileName.Length == 0))
                    {
                        return;
                    }

                }
                else
                {
                    //patientid provided, exists. 

                    //select all scans for patient id.
                    Tuple<LinkedList<int>, LinkedList<DateTime>> scans = db.timestampsForPatientScans(patientid);
                    
                    //select most recent scanid based on timestamp

                    if (scans.Item1.Count == 1)
                    {
                        latestScanTimeID = scans.Item1.First.Value;
                        latestScanTime = scans.Item2.First.Value;
                    }
                    else
                    {

                        scanID = scans.Item1.First;
                        timestamp = scans.Item2.First;

                        while (scanID != null)
                        {
                            var nextID = scanID.Next;
                            var nextTime = timestamp.Next;

                            scans.Item1.Remove(scanID);
                            scans.Item2.Remove(timestamp);

                            scanID = nextID;
                            timestamp = nextTime;

                            if (scanID != null)
                            {
                                if (latestScanTime.CompareTo(timestamp.Value) < 0)
                                {
                                    latestScanTime = timestamp.Value;
                                    latestScanTimeID = scanID.Value;
                                }
                            }
                        }

                    }
            
                    //select pointcloudfilerference based on selected scan id.

                    Tuple<LinkedList<int>, LinkedList<int>, LinkedList<String>, LinkedList<String>, LinkedList<DateTime>> pcScanResults = db.selectQueries.Scans("scanID",latestScanTimeID.ToString());

                    filename = pcScanResults.Item3.First.Value;
                    
                }

                LoadPointCloud();

            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err.ToString());
            }

        }

        public void LoadPointCloud() {

                // Show the window first - keep UI FAST speedy is a stupid word.
                System.Diagnostics.Debug.WriteLine("Showing window");
                shutAnyWindows();

                windowScanner = new ScanLoader((int)ScanLoader.OperationModes.ShowExistingResults);
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

        private void WorkingDirectory_Click(object sender, RoutedEventArgs e)
        {
            //set working directory using a folderbrowsingdialog
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.Description = "Where do you want your working directory to be?";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) { 
                workingDir = dialog.SelectedPath;
                this.shutAnyWindows();
            }

        }

        private void SaveScan_Click(object sender, RoutedEventArgs e)
        {
            windowMeta = new MetaLoader();
            windowMeta.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            windowMeta.Owner = this;
            
            //save specific actions.
            windowMeta.Title = "Attribute scan to whom?";
            windowMeta.button1.Visibility = Visibility.Collapsed;
            windowMeta.button2.Visibility = Visibility.Collapsed;
            windowMeta.button3.Visibility = Visibility.Visible;
            windowMeta.Show();

            windowMeta.Closing += new CancelEventHandler(windowMeta_Closing);
        }

        public List<Tuple<DateTime, double>> getTimeStampsAndVals(int patientID)
        {
            Tuple<LinkedList<int>, LinkedList<DateTime>> data = db.timestampsForPatientScans(patientID);
            LinkedList<int> scanIDs = data.Item1;
            LinkedList<DateTime> times = data.Item2;

            List<Tuple<DateTime, double>> output = new List<Tuple<DateTime, double>>();

            List<DateTime> outputTimes = new List<DateTime>();

            LinkedListNode<DateTime> time = times.First;
            if (time == null) return null;
            while (true)
            {
                outputTimes.Add(time.Value);
                time = time.Next;
                if (time == null) break;
            }

            List<int> outputScans = new List<int>();

            LinkedListNode<int> scanID = scanIDs.First;
            if (scanID == null) return null;
            while (true)
            {
                outputScans.Add(scanID.Value);
                scanID = scanID.Next;
                if (scanID == null) break;
            }

            List<Tuple<DateTime, int>> outputData = new List<Tuple<DateTime, int>>();

            for (int i = 0; i < outputTimes.Count; i++)
            {
                //if this crashes, talk to Bernard cause it works on my machine :p
                double value = db.getScanResult(outputScans[i]).Item4.First.Value;
                output.Add(Tuple.Create(outputTimes[i], value));
            }

            return output;
        }
    }
}