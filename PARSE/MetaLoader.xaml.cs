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
        private Object[] patientslist;

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

            LinkedListNode<int> nodeID = patientsList.Item1.First;
            LinkedListNode<String> nodeName = patientsList.Item2.First;
            LinkedListNode<String> nodeNHSNo = patientsList.Item3.First;

            //populate datagrid

            while (nodeID != null)
            {
                var nextID = nodeID.Next;
                var nextName = nodeName.Next;
                var nextNHSNo = nodeNHSNo.Next;

                listBox1.Items.Add(new { Id = patientsList.Item1.Remove(nodeID), Patientname = patientsList.Item2.Remove(nodeName), Patientnhsno = patientsList.Item3.Remove(nodeNHSNo) } );

                nodeID = nextID;
                nodeName = nextName;
                nodeNHSNo = nextNHSNo;
            }

        }
    }
}
