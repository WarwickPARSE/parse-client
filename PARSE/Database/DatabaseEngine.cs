using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.SqlServerCe;

namespace PARSE
{
    class DatabaseEngine
    {
        SqlCeConnection con;


        public DatabaseEngine()
        {
            dbOpen();

            //AddTest();

            //GetTest();

            Selection newQueries = new Selection(this.con);

            dbClose();
        }

        private void dbOpen()
        {
            try
            {
                con = new SqlCeConnection();
                con.ConnectionString = "Data Source=C:\\Users\\Stefania\\Documents\\Visual Studio 2010\\Projects\\parse-client\\PARSE\\Patients.sdf";
                con.Open();

                Console.WriteLine("open");
            }
            catch (SqlCeException sqlEx)
            {
                Console.WriteLine("sqlEx: " + sqlEx);
            }
        }

        private void dbClose()
        {
            con.Close();

            Console.WriteLine("closed");
        }
    }

    
    class Insertion
    {
        SqlCeConnection con;

        public Insertion(SqlCeConnection connection)
        {
            con = connection;
        }

        public void InsertPatientInformation(int patientID, String name, String dateOfBirth, String nationality, int nhsNo, String address)
        {
            //check what number is the previous patientID and increment by 1?

            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO PatientInformation (patientID, name, dateofbirth, nationality, nhsNo, address) VALUES (@PatientID, @Name, @DateOfBirth, @Nationality, @NhsNo, @Address)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@PatientID", patientID);
            insertQuery.Parameters.Add("@Name", name);
            insertQuery.Parameters.Add("@DateOfBirth", dateOfBirth);
            insertQuery.Parameters.Add("@Nationality", nationality);
            insertQuery.Parameters.Add("@NhsNo", nhsNo);
            insertQuery.Parameters.Add("@Address", address);
            rowsAffected = insertQuery.ExecuteNonQuery();
        }

        public void InsertPatientCondition(int patientConditionID, int patientID, int conditionID, String condition)
        {
            //should have already incremented latest patientConditionID by 1
            //patientID is the ID of the patient currently on the form
            //condition should have been selected from a dropdown list - this here should happen when clicking on Save Changes

            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO PatientCondition (patientConditionID, patientID, conditionID, condition) VALUES (PatientConditionID, PatientID, ConditionID, Condition)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@PatientConditionID", patientConditionID);
            insertQuery.Parameters.Add("@PatientID", patientID);
            insertQuery.Parameters.Add("@ConditionID", conditionID);
            insertQuery.Parameters.Add("@Condition", condition);
            rowsAffected = insertQuery.ExecuteNonQuery();
        }

        public void InsertCondition(int conditionID, String condition, string description)
        {
            //should have already incremented latest conditionID by 1
            //this is for the insertion of known conditions (in general - not related to a patient)

            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO PatientCondition (conditionID, condition, description) VALUES (ConditionID, Condition, Description)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@ConditionID", conditionID);
            insertQuery.Parameters.Add("@Condition", condition);
            insertQuery.Parameters.Add("@Condition", description);
            rowsAffected = insertQuery.ExecuteNonQuery();
        }
    }

    
    class Selection
    {
        SqlCeConnection con;

        public Selection(SqlCeConnection connection)
        {
            con = connection;
        }

        //not necessary hopefully
        public int getLastPatientID()
        {
            int lastID = 0;

            int pSize = 0;
            int[] ids = new int[pSize];

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT patientID FROM PatientInformation";
            selectQuery.Parameters.Clear();
            SqlCeDataReader reader = selectQuery.ExecuteReader();

            while (reader.Read())
            {
                pSize = pSize + 1;
                ids[pSize] = reader.GetInt32(0);
            }
            reader.Close();

            lastID = ids[pSize - 1];

            return lastID;
        }

        public Object[] SelectAllPatients()
        {
            Object[] allPatients = new Object[3];

            int pSize = 0;
            int[] ids = new int[pSize];
            String[] names = new String[pSize];
            int[] nhsNos = new int[pSize];

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT patientID, name, nhsNo FROM PatientInformation";
            selectQuery.Parameters.Clear();
            SqlCeDataReader reader = selectQuery.ExecuteReader();

            while (reader.Read())
            {
                pSize = pSize + 1;
                ids[pSize] = reader.GetInt32(0);
                names[pSize] = reader.GetString(1);
                nhsNos[pSize] = reader.GetInt32(2);
            }
            reader.Close();

            allPatients[0] = ids;
            allPatients[1] = names;
            allPatients[2] = nhsNos;

            return allPatients;
        }

        public Object[] SelectAllConditions()
        {
            Object[] allConditions = new Object[3];

            int cSize = 0;
            int[] ids = new int[cSize];
            String[] conditions = new String[cSize];
            String[] descriptions = new String[cSize];

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM Condition";
            selectQuery.Parameters.Clear();
            SqlCeDataReader reader = selectQuery.ExecuteReader();

            while (reader.Read())
            {
                cSize = cSize + 1;
                ids[cSize] = reader.GetInt32(0);
                conditions[cSize] = reader.GetString(1);
                descriptions[cSize] = reader.GetString(2);
            }
            reader.Close();

            allConditions[0] = ids;
            allConditions[1] = conditions;
            allConditions[2] = descriptions;

            return allConditions;
        }

        public Object[] SelectPatientInformation(int patientID)
        {
            Object[] patient = new Object[6];

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM PatientInformation WHERE patientID LIKE @PatientID";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@PatientID", patientID);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                patient[0] = reader.GetInt32(0);
                patient[1] = reader.GetString(1);
                patient[2] = reader.GetString(2);
                patient[3] = reader.GetString(3);
                patient[4] = reader.GetInt32(4);
                patient[5] = reader.GetString(5);
            }
            reader.Close();

            return patient;
        }

        //generalised method
        public String[] SelectAllTableData(String tableName, String column, int value)
        {
            String[] tableData = new String[2];
            List<String> test = new List<String>();
            int counter = 0;

            tableData[0] = "Default";
            tableData[1] = "Default";

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM @TableName WHERE @Column LIKE @Value";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@TableName", tableName);
            selectQuery.Parameters.Add("@Column", column);
            selectQuery.Parameters.Add("@Value", value);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {

                test.Add(reader.GetString(counter));
                counter++;
                tableData[0] = reader.GetString(0);
                tableData[1] = reader.GetString(1);
            }
            Console.WriteLine(tableData[0], tableData[1]);

            reader.Close();

            return tableData;
        }
    }
}
