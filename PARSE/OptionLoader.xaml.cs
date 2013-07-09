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
    /// Interaction logic for OptionLoader.xaml
    /// </summary>
    public partial class OptionLoader : Window
    {
        public OptionLoader()
        {
            InitializeComponent();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void volumeBtn_Click(object sender, RoutedEventArgs e)
        {
            ScanLoader newScanWindow = new ScanLoader((int)ScanLoader.OperationModes.CaptureNewCloud);
            newScanWindow.Owner = this;
            newScanWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            newScanWindow.Show();
        }

        private void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            PatientLoader newScanWindow = new PatientLoader(true);
            newScanWindow.Show();
            newScanWindow.recordedscans.Visibility = Visibility.Visible;
            newScanWindow.patientEntry.SelectedIndex = 2;
            newScanWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        }
    }
}
