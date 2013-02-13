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

namespace PARSE
{
    /// <summary>
    /// Interaction logic for PatientLoader.xaml
    /// </summary>
    public partial class PatientLoader : Window
    {
        public PatientLoader()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PatientLoader_Loaded);
        }

        private void PatientLoader_Loaded(object Sender, RoutedEventArgs e)
        {
            //place relative to coreloader
            this.Top = this.Owner.Top + 70;
            this.Left = this.Owner.Left + 20;
            this.Width = this.Owner.OwnedWindows[0].Width + 20;
            this.Height = this.Owner.OwnedWindows[0].Height;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
