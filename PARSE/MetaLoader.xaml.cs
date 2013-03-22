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

            Selection patients = db.selectQueries;

            patientslist = patients.SelectAllPatients();
            
            System.Diagnostics.Debug.WriteLine(patientslist[0]);

        }
    }
}
