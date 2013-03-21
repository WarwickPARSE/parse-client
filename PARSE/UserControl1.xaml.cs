
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PARSE
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml Loading Widget
    /// </summary>
    public partial class LoadingWidget : UserControl
    {

        private delegate void NoArgDelegate();

        public LoadingWidget()
        {
            InitializeComponent();
            Console.WriteLine("Loading Widget Initialized!");
        }

        public void UpdateProgress()
        {
            progressBar1.Value += 1;
            Text_Progress.Text = progressBar1.Value + "%";
        }

        public void UpdateProgressBy(int progress)
        {
            for (int p = 0; p < progress; p ++)
                this.SetProgress((int)(1 + progressBar1.Value));
        }
       
        private void SetProgress(int progress)
        {
            progressBar1.Value = progress;
            Text_Progress.Text = progress.ToString() + "%";
        }

        public void ReportProgress(int progress, object data)
        {
            this.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Render,
                (Action)(() =>
                {
                    if (progress > -1)
                    {
                        this.UpdateProgressBy(progress);
                    }

                    if (data is string)
                    {
                        System.Diagnostics.Debug.WriteLine("%" + progress + " -> " + progressBar1.Value + " @ " + (string)data);
                    }
                }));
        }
    }
}
