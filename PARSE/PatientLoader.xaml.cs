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
        private int currentID;
        private bool existing;

        public PatientLoader(bool existing)
        {
            InitializeComponent();

            this.existing = existing;

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
            Selection getID = db.selectQueries;

            //hide existing patient labels
            this.patientIDExisting.Visibility = Visibility.Collapsed;
            this.patientNameExisting.Visibility = Visibility.Collapsed;
            this.patientNhsNoExisting.Visibility = Visibility.Collapsed;
            this.patientweightExisting.Visibility = Visibility.Collapsed;
            this.patientDOBExisting.Visibility = Visibility.Collapsed;
            this.patientNationalityExisting.Visibility = Visibility.Collapsed;
            this.patientAddressExisting.Visibility = Visibility.Collapsed;

            //hide irrelevant tabs
            this.recordedscans.Visibility = Visibility.Collapsed;
            this.conditiondetail.Visibility = Visibility.Collapsed;

            //enable/disable appropriate controls
            this.patientIDText.IsEnabled = false;
            this.patientIDText.Text = (getID.getLastPatientID()+1).ToString();
        }

        private void PatientLoaderExisting_Loaded(object Sender, RoutedEventArgs e)
        {
            //TODO: This will eventually be given the relevant select query from the database call.
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Selection select = db.selectQueries;
            currentID = select.getLastPatientID();

            //hide existing patient labels
            this.patientIDExisting.Visibility = Visibility.Visible;
            this.patientNameExisting.Visibility = Visibility.Visible;
            this.patientNhsNoExisting.Visibility = Visibility.Visible;
            this.patientweightExisting.Visibility = Visibility.Visible;
            this.patientDOBExisting.Visibility = Visibility.Visible;
            this.patientNationalityExisting.Visibility = Visibility.Visible;
            this.patientAddressExisting.Visibility = Visibility.Visible;

            this.patientIDText.Visibility = Visibility.Collapsed;
            this.nameText.Visibility = Visibility.Collapsed;
            this.nhsNoText.Visibility = Visibility.Collapsed;
            this.weightText.Visibility = Visibility.Collapsed;
            this.dobText.Visibility = Visibility.Collapsed;
            this.nationalityText.Visibility = Visibility.Collapsed;
            this.addressText.Visibility = Visibility.Collapsed;

            //alter relevant ui control content for patient existing.
            this.proceedCon.Content = "Edit Details";

            //Select patient information provided by id (to be added)
            try
            {

                select = db.selectQueries;

                dbReceiver = select.SelectPatientInformation(currentID);
                this.label1.Content = dbReceiver[1];
                this.patientEntryStatus.Text = "Patients database: " + db.con.ConnectionString;

                for (int item = 0; item < dbReceiver.Length; item++)
                {
                    switch (item)
                    {
                        case 0: this.patientIDExisting.Content = dbReceiver[item]; break;
                        case 1: this.patientNameExisting.Content = dbReceiver[item]; break;
                        case 2: this.patientDOBExisting.Content = Convert.ToString(dbReceiver[item]); break;
                        case 3: this.patientNationalityExisting.Content = dbReceiver[item]; break;
                        case 4: this.patientNhsNoExisting.Content = dbReceiver[item]; break;
                        case 5: this.patientAddressExisting.Text = dbReceiver[item].ToString(); break;
                        case 6: this.patientweightExisting.Content = dbReceiver[item]; break;
                    }
                }

            catch (Exception err)
            {

                System.Diagnostics.Debug.WriteLine(err.ToString());

            }
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

        private void proceedCon_Click(object sender, RoutedEventArgs e)
        {
            if(!existing) {
                this.conditiondetail.Visibility = Visibility.Visible;
                this.patientEntry.SelectedIndex = 1;

            } else if (existing && this.proceedCon.Content=="Proceed -->") {
                this.conditiondetail.Visibility = Visibility.Visible;
                this.patientEntry.SelectedIndex = 1;

            } else {
                this.proceedCon.Content = "Proceed -->";
                this.proceedsave.Content = "Save Changes";

                //hide visibility of data entry form
                this.patientIDText.Visibility = Visibility.Visible;
                this.nameText.Visibility = Visibility.Visible;
                this.nhsNoText.Visibility = Visibility.Visible;
                this.weightText.Visibility = Visibility.Visible;
                this.dobText.Visibility = Visibility.Visible;
                this.nationalityText.Visibility = Visibility.Visible;
                this.addressText.Visibility = Visibility.Visible;

                //Check for existence of record and populate form.
                //show existing patient labels
                this.patientIDExisting.Visibility = Visibility.Collapsed;
                this.patientNameExisting.Visibility = Visibility.Collapsed;
                this.patientNhsNoExisting.Visibility = Visibility.Collapsed;
                this.patientweightExisting.Visibility = Visibility.Collapsed;
                this.patientDOBExisting.Visibility = Visibility.Collapsed;
                this.patientNationalityExisting.Visibility = Visibility.Collapsed;
                this.patientAddressExisting.Visibility = Visibility.Collapsed;
            }
        }

        private void proceedSave_Click(object sender, RoutedEventArgs e)
        {
            //save patient data to database (include the multiple conditions as above)

            Insertion insertPatient = db.insertQueries;
            Selection getID = db.selectQueries;

            //remove content from address box (as it's an annoying rich textbox control)
            TextRange textRange = new TextRange(
                // TextPointer to the start of content in the RichTextBox.
                this.addressText.Document.ContentStart,
                // TextPointer to the end of content in the RichTextBox.
                this.addressText.Document.ContentEnd
            );

            insertPatient.InsertPatientInformation(0, this.nameText.Text, this.dobText.Text, this.nationalityText.Text, Convert.ToInt32(this.nhsNoText.Text), textRange.Text, this.weightText.Text);

            //We then need to get back the id of the recorded inserted so it can be stored on a volatile basis.
            currentID = getID.getLastPatientID();

            db.con.Close();

            //load windows for basic volume scanning procedure
            if (!existing)
            {
                OptionLoader windowOption = new OptionLoader();
                windowOption.Show();
            }

            this.Close();
        }
    }
}
