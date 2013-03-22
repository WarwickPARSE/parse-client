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
        private DatabaseEngine db = new DatabaseEngine();
        private Object[] dbReceiver;

        public PatientLoader(bool existing)
        {
            InitializeComponent();

            if (existing)
            {
                this.Loaded += new RoutedEventHandler(PatientLoaderExisting_Loaded);
            }
            else
            {
                this.Loaded += new RoutedEventHandler(PatientLoader_Loaded);
            }
        }

        private void PatientLoader_Loaded(object Sender, RoutedEventArgs e)
        {
            //place relative to coreloader
            this.Top = this.Owner.Top + 70;
            this.Left = this.Owner.Left + 20;
            this.Width = this.Owner.OwnedWindows[0].Width + 20;
            this.Height = this.Owner.OwnedWindows[0].Height;

            //hide existing patient labels
            this.patientIDExisting.Visibility = Visibility.Collapsed;
            this.patientNameExisting.Visibility = Visibility.Collapsed;
            this.patientNhsNoExisting.Visibility = Visibility.Collapsed;
            this.patientDOBExisting.Visibility = Visibility.Collapsed;
            this.patientNationalityExisting.Visibility = Visibility.Collapsed;
            this.patientAddressExisting.Visibility = Visibility.Collapsed;
        }

        private void PatientLoaderExisting_Loaded(object Sender, RoutedEventArgs e)
        {
            //TODO: This will eventually be given the relevant select query from the database call.

            //initialize database with selection connection object
            Selection select = new Selection(db.con);

            //place relative to coreloader
            this.Top = this.Owner.Top + 70;
            this.Left = this.Owner.Left + 20;
            this.Width = this.Owner.OwnedWindows[0].Width + 20;
            this.Height = this.Owner.OwnedWindows[0].Height;

            //hide visibility of data entry form
            this.patientIDText.Visibility = Visibility.Collapsed;
            this.nameText.Visibility = Visibility.Collapsed;
            this.nhsNoText.Visibility = Visibility.Collapsed;
            this.dobText.Visibility = Visibility.Collapsed;
            this.nationalityText.Visibility = Visibility.Collapsed;
            this.addressText.Visibility = Visibility.Collapsed;

            //Check for existence of record and populate form.
            //show existing patient labels
            this.patientIDExisting.Visibility = Visibility.Visible;
            this.patientNameExisting.Visibility = Visibility.Visible;
            this.patientNhsNoExisting.Visibility = Visibility.Visible;
            this.patientDOBExisting.Visibility = Visibility.Visible;
            this.patientNationalityExisting.Visibility = Visibility.Visible;
            this.patientAddressExisting.Visibility = Visibility.Visible;

            /*dbReceiver = select.SelectPatientInformation(1);
            this.label1.Content = dbReceiver[1];
            this.patientEntryStatus.Text = "Patients database: " + db.con.ConnectionString;

            for (int item = 0; item < dbReceiver.Length; item++) {
                switch (item)
                {
                    case 0: this.patientIDExisting.Content = dbReceiver[item]; break;
                    case 1: this.patientNameExisting.Content = dbReceiver[item]; break;
                    case 2: this.patientDOBExisting.Content = dbReceiver[item]; break;
                    case 3: this.patientNationalityExisting.Content = dbReceiver[item]; break;
                    case 4: this.patientNhsNoExisting.Content = dbReceiver[item]; break;
                    case 5: this.patientAddressExisting.Text = dbReceiver[item].ToString(); break;
                }
            }*/

        }

        void NewScan_Click(Object sender, RoutedEventArgs e)
        {
            MeasurementLoader NewScanWindow = new MeasurementLoader();
            NewScanWindow.Show();
            // TODO Refresh when results saved

        }
        void RemoveScan_Click(object sender, RoutedEventArgs e)
        {

        }
        void ScanAll_Click(object sender, RoutedEventArgs e)
        {

        }
        void ScanSelected_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
