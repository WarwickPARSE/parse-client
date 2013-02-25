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
    public partial class RuntimeLoader : Window
    {
        public RuntimeLoader()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(RuntimeLoader_Loaded);
        }

        private void RuntimeLoader_Loaded(object Sender, RoutedEventArgs e)
        {
            //place relative to coreloader
            this.Left = this.Owner.Left + 20;
            this.Width = (this.Owner.OwnedWindows[0].Width * 2.075);
            this.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - (this.Owner.OwnedWindows[0].Width/1.25);
            this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - this.Height;
            this.textBox1.Width = this.Width - 20;
            this.textBox1.Height = this.Height - 75;
            //set console out to this control

            TraceListener debugListener = new MyTraceListener(textBox1);
            Debug.Listeners.Add(debugListener);
            //Trace.Listeners.Add(debugListener);
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

        public void visualisePlanes(List<List<Point3D>> planes)
        {
            File.Delete("./output.csv");
            
            System.Diagnostics.Debug.WriteLine("Number of caught planes: " + planes.Count);

            double xmin = 0;
            double xmax = 0;
            double zmin = 0;
            double zmax = 0;

            //currently taking the midpoint on the body (waist/stomach area)
            int i = planes.Count / 2;
            PointSorter.rotSort(planes[i]);

            double[] x = new double[planes[i].Count];
            double[] z = new double[planes[i].Count];

                for (int j = 0; j < planes[i].Count; j++) {

                        //Boundary check of points.
                        if (planes[i][j].X > xmax)
                        {
                            xmax = planes[i][j].X;
                        }

                        if (planes[i][j].Z > zmax)
                        {
                            zmax = planes[i][j].Z;
                        }

                        if (planes[i][0].X < xmin)
                        {
                            xmin = planes[i][0].X;
                        }
                        if (planes[i][j].X < xmin)
                        {
                            xmin = planes[i][j].X;
                        }

                        if (planes[i][0].Z < zmin)
                        {
                            zmin = planes[i][0].Z;
                        }
                        if (planes[i][j].Z < zmin)
                        {
                            zmin = planes[i][j].Z;
                        }

                        //write points to output.csv file
                        using (StreamWriter w = File.AppendText("./output.csv"))
                       {
                            w.WriteLine(planes[i][j].X + "," + planes[i][j].Z);
                            w.Flush();
                            w.Close();
                       }

                       //assign to arrays
                        x[j] = planes[i][j].X;
                        z[j] = planes[i][j].Z;

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
            Environment.Exit(1);
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
