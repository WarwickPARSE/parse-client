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
            System.Diagnostics.Debug.WriteLine("Number of caught planes: " + planes.Count);

            double xmin = 0;
            double xmax = 0;
            double zmin = 0;
            double zmax = 0;

            for (int i = 0; i < planes.Count; i++)
            {

            //int i = planes.Count / 2;    
            VisCanvas.Children.RemoveRange(0, VisCanvas.Children.Count);

                for (int j = 0; j < planes[i].Count; j++) {

                        Ellipse circle = new Ellipse();
                        circle.Width = 1;
                        circle.Height = 1;
                        circle.StrokeThickness = 0.1;
                        circle.Stroke = Brushes.Black;

                        VisCanvas.Children.Add(circle);

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

                        using (StreamWriter w = File.AppendText("output.txt"))
                        {
                            w.WriteLine(planes[i][j].X + "," + planes[i][j].Y);
                        }

                        Canvas.SetLeft(circle, 230+ (planes[i][j].X * 100));
                        Canvas.SetTop(circle, 100+(planes[i][j].Y * 5));
                }

                using (StreamWriter w = File.AppendText("output.txt"))
                {
                    w.WriteLine("end of plane, end of plane");
                }
            }

            System.Diagnostics.Debug.WriteLine("Planes visualised");
            System.Diagnostics.Debug.WriteLine("xmin: " + xmin);
            System.Diagnostics.Debug.WriteLine("zmin: " + zmin);
            System.Diagnostics.Debug.WriteLine("xmax: " + xmax);
            System.Diagnostics.Debug.WriteLine("zmax: " + zmax);
            System.Diagnostics.Debug.WriteLine("points on canvas: " + VisCanvas.Children.Count);
        }

        private void VisCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point location = Mouse.GetPosition(VisCanvas);
            st.CenterX = location.X;
            st.CenterY = location.Y;
            st.ScaleX *= 1.5;
            st.ScaleY *= 1.5;
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
