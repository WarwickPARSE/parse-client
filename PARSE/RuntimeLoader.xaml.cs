using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.IO;

//using System.Windows.Controls.DataVisualization.Charting;

namespace PARSE
{
    /// <summary>
    /// Interaction logic for RuntimeLoader.xaml
    /// </summary>
    /// 

    public partial class RuntimeLoader : Window
    {

        //persistently store our list of planes
        private List<List<Point3D>> storedPlanes;

        public RuntimeLoader()
        {
            InitializeComponent();
            storedPlanes = new List<List<Point3D>>();
            this.Loaded += new RoutedEventHandler(RuntimeLoader_Loaded);
        }

        private void RuntimeLoader_Loaded(object Sender, RoutedEventArgs e)
        {
            //place relative to coreloader
            this.Left = this.Owner.Left + 20;
            this.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - 30;
            this.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - (this.Owner.OwnedWindows[0].Width/1.25);
            this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - this.Height;
            this.textBox1.Width = this.Width - 20;
            this.textBox1.Height = this.Height - 75;

            //set console out to this control
            TraceListener debugListener = new MyTraceListener(textBox1);
            Debug.Listeners.Add(debugListener);
            //Trace.Listeners.Add(debugListener);

            //check if a scan event is in place

            if (storedPlanes.Count == 0)
            {
                //in the case when we have no planes to show, modify the ui.
                bodyimg.Visibility = Visibility.Collapsed;
                planeNo.Visibility = Visibility.Collapsed;
                viewborder.Visibility = Visibility.Collapsed;
                hvpcanvas.Visibility = Visibility.Hidden;
                planeChooser.Visibility = Visibility.Collapsed;
                vollabel.Visibility = Visibility.Collapsed;
                voLconclabel.Visibility = Visibility.Collapsed;

                voloutput.Visibility = Visibility.Collapsed;
                heightlabel.Visibility = Visibility.Collapsed;
                heightoutput.Visibility = Visibility.Collapsed;
                otherlabel.Visibility = Visibility.Collapsed;

                scanno.Visibility = Visibility.Collapsed;
                scantime.Visibility = Visibility.Collapsed;
                scanfileref.Visibility = Visibility.Collapsed;
                scanvoxel.Visibility = Visibility.Collapsed;
                maxarea.Visibility = Visibility.Collapsed;
                totalarea.Visibility = Visibility.Collapsed;
                totalperimiter.Visibility = Visibility.Collapsed;

            }
            else
            {

            }
        }

        public void sendMessageToOutput(String type, String message) {

            try
            {
                String curtime = "[" + DateTime.Now.ToString("T") + "]";

                //case statements for different types of messages to go here.
                textBox1.AcceptsReturn = true;
                textBox1.AppendText(curtime + "[" + type + "]: " + message+"\u2028");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("[CRITICAL]: Output window failed to update");
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        public void visualisePlanes(List<List<Point3D>> planes, double planeIndex)
        {
            File.Delete("./output.csv");

            bodyimg.Visibility = Visibility.Visible;
            planeNo.Visibility = Visibility.Visible;
            viewborder.Visibility = Visibility.Visible;
            hvpcanvas.Visibility = Visibility.Visible;
            planeChooser.Visibility = Visibility.Visible;
            vollabel.Visibility = Visibility.Visible;
            voLconclabel.Visibility = Visibility.Visible;
            voloutput.Visibility = Visibility.Visible;
            heightlabel.Visibility = Visibility.Visible;
            heightoutput.Visibility = Visibility.Visible;
            otherlabel.Visibility = Visibility.Visible;
            scanno.Visibility = Visibility.Visible;
            scantime.Visibility = Visibility.Visible;
            scanfileref.Visibility = Visibility.Visible;
            scanvoxel.Visibility = Visibility.Visible;
            maxarea.Visibility = Visibility.Visible;
            totalarea.Visibility = Visibility.Visible;
            totalperimiter.Visibility = Visibility.Visible;

            noresults.Visibility = Visibility.Collapsed;
            newscan.Visibility = Visibility.Collapsed;

            planeNo.Text = "Plane Outline: " + (int) planeIndex;
            
            System.Diagnostics.Debug.WriteLine("Number of caught planes: " + planes.Count);

            if (storedPlanes.Count == 0)
            {
                storedPlanes = planes;
                storedPlanes.Reverse();
            }

            double xmin = 0;
            double xmax = 0;
            double zmin = 0;
            double zmax = 0;

            //currently taking the midpoint on the body (waist/stomach area)
            int i = (int) planeIndex;
            PointSorter.rotSort(storedPlanes[i]);

            double[] x = new double[storedPlanes[i].Count];
            double[] z = new double[storedPlanes[i].Count];

                for (int j = 0; j < storedPlanes[i].Count; j++) {

                        //Boundary check of points.
                        if (storedPlanes[i][j].X > xmax)
                        {
                            xmax = storedPlanes[i][j].X;
                        }

                        if (storedPlanes[i][j].Z > zmax)
                        {
                            zmax = storedPlanes[i][j].Z;
                        }

                        if (storedPlanes[i][0].X < xmin)
                        {
                            xmin = storedPlanes[i][0].X;
                        }
                        if (storedPlanes[i][j].X < xmin)
                        {
                            xmin = storedPlanes[i][j].X;
                        }

                        if (storedPlanes[i][0].Z < zmin)
                        {
                            zmin = storedPlanes[i][0].Z;
                        }
                        if (storedPlanes[i][j].Z < zmin)
                        {
                            zmin = storedPlanes[i][j].Z;
                        }

                        //write points to output.csv file
                        using (StreamWriter w = File.AppendText("./output.csv"))
                       {
                            w.WriteLine(storedPlanes[i][j].X + "," + storedPlanes[i][j].Z);
                            w.Flush();
                            w.Close();
                       }

                       //assign to arrays
                        x[j] = storedPlanes[i][j].X;
                        z[j] = storedPlanes[i][j].Z;

                }

                //write points to plane renderer class for visualisation.
                this.DataContext = new PlaneVisualisation(x,z);


                using (StreamWriter w = File.AppendText("./output.csv"))
                {
                    w.WriteLine("end of plane, end of plane");
                    w.Flush();
                    w.Close();
                }
                Console.WriteLine("end of plane, end of plane");

            System.Diagnostics.Debug.WriteLine("Planes visualised");
            System.Diagnostics.Debug.WriteLine("xmin: " + xmin);
            System.Diagnostics.Debug.WriteLine("zmin: " + zmin);
            System.Diagnostics.Debug.WriteLine("xmax: " + xmax);
            System.Diagnostics.Debug.WriteLine("zmax: " + zmax);
           // Environment.Exit(1);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void planeChooser_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            visualisePlanes(storedPlanes, e.NewValue);
        }
    }

    public class MyTraceListener : TraceListener
    {
        private System.Windows.Controls.RichTextBox output;

        public MyTraceListener(System.Windows.Controls.RichTextBox output)
        {
            this.Name = "Trace";
            this.output = output;
        }


        public override void Write(string message)
        {

            Action append = delegate()
            {
                output.AppendText(string.Format("[{0}] ", DateTime.Now.ToString("T")));
                output.AppendText(message);
                output.ScrollToEnd();
            };
            if (output.Dispatcher.CheckAccess())
            {
                output.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal, append);
            }
            else
            {
                append();
            }

        }

        public override void WriteLine(string message)
        {
            Write("[Debug]: " + message + "\u2028");
        }
    }

    }
