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

namespace PARSE.Tracking
{
    /// <summary>
    /// Interaction logic for MeasurementRecord.xaml
    /// </summary>
    public partial class MeasurementRecord : UserControl
    {
        public MeasurementRecord()
        {
            InitializeComponent();
        }

        public MeasurementRecord(String location, String firstDate, String lastDate, String lastValue)
        {
            InitializeComponent();

            label_Location.Content += location;
            label_First.Content += firstDate;
            label_Recent.Content += lastDate;
            label_Value.Content += lastValue;
        }

        private void ContextMenu_NewMeasurement(object sender, RoutedEventArgs e)
        {

        }

        private void ContextMenu_RemoveMeasurementLocation(object sender, RoutedEventArgs e)
        {

        }
        
        private void ContextMenu_ShowHistory(object sender, RoutedEventArgs e)
        {

        }
    }
}
