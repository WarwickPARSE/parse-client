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
using System.Diagnostics;
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
            this.Top = this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - this.Height + 20;
            this.Left = this.Owner.Left + 20;
            this.Width = (this.Owner.OwnedWindows[0].Width * 2.075);
            this.Height = this.Owner.OwnedWindows[0].Height - 125;
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
            }
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
