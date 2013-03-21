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
using System.Windows.Threading;
using System.ComponentModel;

namespace PARSE
{
    /// <summary>
    /// Interaction logic for DebugLoader.xaml
    /// </summary>
    public partial class DebugLoader : Window
    {
        public DebugLoader()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(DebugLoader_Loaded);
            this.Closing += new CancelEventHandler(Window_Closing);
        }

        private void DebugLoader_Loaded(object Sender, RoutedEventArgs e)
        {
            //place relative to coreloader
            this.Left = this.Owner.Left + 20;
            this.Top = this.Owner.Top + 20;
            this.debugBox.Width = this.Width - 20;
            this.debugBox.Height = this.Height - 75;

            //set console out to this control
            TraceListener debugListener = new MyTraceListener(debugBox);
            Debug.Listeners.Add(debugListener);
            //Trace.Listeners.Add(debugListener);
        }

        public void sendMessageToOutput(String type, String message)
        {

            try
            {
                String curtime = "[" + DateTime.Now.ToString("T") + "]";

                //case statements for different types of messages to go here.
                debugBox.AcceptsReturn = true;
                debugBox.AppendText(curtime + "[" + type + "]: " + message + "\u2028");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("[CRITICAL]: Output window failed to update");
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; 
            this.Hide();
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