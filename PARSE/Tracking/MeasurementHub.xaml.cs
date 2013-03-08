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

namespace PARSE.Tracking
{
    /// <summary>
    /// Interaction logic for MeasurementHub.xaml
    /// </summary>
    public partial class MeasurementHub : Window
    {
        public MeasurementHub()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //MeasurementStack.Children.Add(new MeasurementRecord("Arm", "July", "August", "5"));
            //MeasurementStack.Children.Add(new MeasurementRecord("Brain", "July", "November", "4.1"));
            //MeasurementStack.Children.Add(new MeasurementRecord("Leg", "July", "February", "3.3"));
        }
    }
}
