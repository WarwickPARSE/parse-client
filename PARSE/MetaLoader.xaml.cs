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
    /// Interaction logic for MetaLoader.xaml
    /// </summary>

    public partial class MetaLoader : Window
    {

        private DatabaseEngine db = new DatabaseEngine();
        Tuple<int, String, String> selectedRecord;
        LinkedListNode<int> nodeID;
        LinkedListNode<String> nodeName;
        LinkedListNode<String> nodeNHSNo;

        public MetaLoader()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MetaLoader_Loaded);
        }


        void MetaLoader_Loaded(object sender, RoutedEventArgs e)
        {
            /// <summary>
            /// Calls patient information records from the database
            /// </summary>

            db.dbOpen();

            //access all patients from database.
            Tuple<LinkedList<int>,LinkedList<String>,LinkedList<string>> patientsList = db.getAllPatients();

            nodeID = patientsList.Item1.First;
            nodeName = patientsList.Item2.First;
            nodeNHSNo = patientsList.Item3.First;

            //populate datagrid

            while (nodeID != null)
            {
                var nextID = nodeID.Next;
                var nextName = nodeName.Next;
                var nextNHSNo = nodeNHSNo.Next;

                patientsList.Item1.Remove(nodeID);
                patientsList.Item2.Remove(nodeName);
                patientsList.Item3.Remove(nodeNHSNo);

                //null check when at end of list
                if (nodeID != null)
                {
                    var nodes = new { Id = nodeID.Value, Patientname = nodeName.Value, Patientnhsno = nodeNHSNo.Value };
                    listBox1.Items.Add(nodes);
                }

                nodeID = nextID;
                nodeName = nextName;
                nodeNHSNo = nextNHSNo;

            }

        }

        public List<Tuple<DateTime,double>> getTimeStampsAndVals(int patientID)
        {
            Tuple<LinkedList<int>, LinkedList<DateTime>> data = db.timestampsForPatientScans(patientID);
            LinkedList<int> scanIDs = data.Item1;
            LinkedList<DateTime> times = data.Item2;

            List<Tuple<DateTime, double>> output = new List<Tuple<DateTime, double>>();

            List<DateTime> outputTimes = new List<DateTime>();

            LinkedListNode<DateTime> time = times.First;
            if (time == null) return null;
            while (true)
            {
                outputTimes.Add(time.Value);
                time = time.Next;
                if (time == null) break;
            }

            List<int> outputScans = new List<int>();
            
            LinkedListNode<int> scanID = scanIDs.First;
            if (scanID == null) return null;
            while (true)
            {
                outputScans.Add(scanID.Value);
                scanID = scanID.Next;
                if (scanID == null) break;
            }
            
            List<Tuple<DateTime, int>> outputData = new List<Tuple<DateTime, int>>();
                        
            for (int i = 0; i < outputTimes.Count; i++)
            {
                //if this crashes, talk to Bernard cause it works on my machine :p
                double value = db.getScanResult(outputScans[i]).Item4.First.Value;
                output.Add(Tuple.Create(outputTimes[i], value));
            }
            
            return output;
        }
        
        public Tuple<int, String, String> returnSelectedRecord()
        {
            return selectedRecord;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //retrieve as an anonymous object type
            Object activeRecord = listBox1.SelectedItems[0];

            //access attributes through reflection
            dynamic d = activeRecord;
            object id = d.Id;
            object nm = d.Patientname;
            object nhs = d.Patientnhsno;

            //cast attributes to a tuple
            selectedRecord = new Tuple<int, String, String>(Convert.ToInt32(id), nm.ToString(), nhs.ToString());

            //close the window
            this.Close();

            (Owner as CoreLoader).LoadPointCloudFromFile(Convert.ToInt32(id));
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            (Owner as CoreLoader).LoadPointCloudFromFile();            
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
