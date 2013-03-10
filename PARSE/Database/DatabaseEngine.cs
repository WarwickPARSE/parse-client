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
        public SqlCeConnection con;

        Insertion insertQueries;
        Selection selectQueries;

        public DatabaseEngine()
        {
            insertQueries = new Insertion(this.con);
            selectQueries = new Selection(this.con);
        }

        private void dbOpen()
        {
            try
            {
                this.con = new SqlCeConnection();
                this.con.ConnectionString = "Data Source=|DataDirectory|\\Patients.sdf";
                this.con.Open();

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

        //Insert methods for all tables

        //Type 1 = tables that need a specialised query
        public void insertPatientInformation(int patientID, String name, String dateOfBirth, String nationality, int nhsNo, String address)
        {
            dbOpen();

            insertQueries.patientInformation(patientID, name, dateOfBirth, nationality, nhsNo, address);

            dbClose();
        }

        public void insertScanLocations(int scanLocID, String boneName, String jointName1, String jointName2, double distJoint1, double distJoint2, double jointsDist, DateTime timestamp)
        {
            dbOpen();

            insertQueries.scanLocations(scanLocID, boneName, jointName1, jointName2, distJoint1, distJoint2, jointsDist, timestamp);

            dbClose();
        }

        public void insertScans(int scanID, String pointCloudFileReference, DateTime timestamp)
        {
            dbOpen();

            insertQueries.scans(scanID, pointCloudFileReference, timestamp);

            dbClose();
        }

        public void insertRecords(int recordID, int scanID, int scanTypeID, double value)
        {
            dbOpen();

            insertQueries.records(recordID, scanID, scanTypeID, value);

            dbClose();
        }

        //Type 2 = tables containing 1 identifier and 2 text fields

        //contains the different types of conditions
        public void insertConditions(int conditionID, String condition, String description)
        {
            dbOpen();

            insertQueries.type2("Conditions", "conditionID", "condition", "description", conditionID, condition, description);

            dbClose();
        }

        //contains the different types of scans
        public void insertScanTypes(int scanTypeID, String scanType, String description)
        {
            dbOpen();

            insertQueries.type2("ScanTypes", "scanTypeID", "scanType", "description", scanTypeID, scanType, description);

            dbClose();
        }

        //Type 3 = tables containing 3 identifiers

        public void insertPatientCondition(int patientConditionID, int patientID, int conditionID)
        {
            dbOpen();

            insertQueries.type3("PatientCondition", "patientConditionID", "patientID", "conditionID", patientConditionID, patientID, conditionID);

            dbClose();
        }

        public void insertPatientScans(int patientScanID, int patientID, int scanTypeID)
        {
            dbOpen();

            insertQueries.type3("PatientScans", "patientScanID", "patientID", "scanTypeID", patientScanID, patientID, scanTypeID);

            dbClose();
        }

        public void insertPointRecognitionScans(int pointRecognitionScanID, int patientID, int scanLocID)
        {
            dbOpen();

            insertQueries.type3("PointRecognitionScans", "pointRecognitionScanID", "patientID", "scanLocID", pointRecognitionScanID, patientID, scanLocID);

            dbClose();
        }
    }

    
    class Insertion
    {
        SqlCeConnection con;

        public Insertion(SqlCeConnection connection)
        {
            con = connection;
        }

        //Insertion Queries for Type 1 tables

        public void patientInformation(int patientID, String name, String dateOfBirth, String nationality, int nhsNo, String address)
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

        public void scanLocations(int scanLocID, String boneName, String jointName1, String jointName2, double distJoint1, double distJoint2, double jointsDist, DateTime timestamp)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO ScanLocations ((scanLocID, boneName, jointName1, jointName2, distJoint1, distJoint2, jointsDist, timestamp) VALUES (@ScanLocID, @BoneName, @JointName1, @JointName2, @DistJoint1, @DistJoint2, @JointsDist, @Timestamp)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@ScanLocID", scanLocID);
            insertQuery.Parameters.Add("@BoneName", boneName);
            insertQuery.Parameters.Add("@JointName1", jointName1);
            insertQuery.Parameters.Add("@JointName2", jointName2);
            insertQuery.Parameters.Add("@DistJoint1", distJoint1);
            insertQuery.Parameters.Add("@DistJoint2", distJoint2);
            insertQuery.Parameters.Add("@JointsDist", jointsDist);
            insertQuery.Parameters.Add("@Timestamp", timestamp.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            rowsAffected = insertQuery.ExecuteNonQuery();
        }

        public void scans(int scanID, String pointCloudFileReference, DateTime timestamp)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO ScanLocations ((scanID, pointCloudFileReference, timestamp) VALUES (@ScanID, @pointCloudFileReference, @Timestamp)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@ScanID", scanID);
            insertQuery.Parameters.Add("@PointCloudFileReference", pointCloudFileReference);
            insertQuery.Parameters.Add("@Timestamp", timestamp.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            rowsAffected = insertQuery.ExecuteNonQuery();
        }

        public void records(int recordID, int scanID, int scanTypeID, double value)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO Records (recordID, scanID, scanTypeID, value) VALUES (@RecordID, @ScanID, @ScanTypeID, @Value)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@RecordID", recordID);
            insertQuery.Parameters.Add("@ScanID", scanID);
            insertQuery.Parameters.Add("@ScanTypeID", scanTypeID);
            insertQuery.Parameters.Add("@Value", value);
            rowsAffected = insertQuery.ExecuteNonQuery();
        }

        //Insertion Query for Type 2 tables

        public void type2(String tableName, String idCol, String textCol1, String textCol2, int id, String text1, String text2)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO @TableName (@IdCol, @TextCol1, @TextCol2) VALUES (@Id, @Text1, @Text2)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@TableName", tableName);
            insertQuery.Parameters.Add("@IdCol", idCol);
            insertQuery.Parameters.Add("@TextCol1", textCol1);
            insertQuery.Parameters.Add("@TextCol2", textCol2);
            insertQuery.Parameters.Add("@Id", id);
            insertQuery.Parameters.Add("@Text1", text1);
            insertQuery.Parameters.Add("@Text2", text2);
            rowsAffected = insertQuery.ExecuteNonQuery();
        }

        //Insertion Query for Type 3 tables

        public void type3(String tableName, String idCol1, String idCol2, String idCol3, int id1, int id2, int id3)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO @TableName (@IdCol1, @IdCol2, @IdCol3) VALUES (@Id1, @Id2, @Id3)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@TableName", tableName);
            insertQuery.Parameters.Add("@IdCol1", idCol1);
            insertQuery.Parameters.Add("@IdCol2", idCol2);
            insertQuery.Parameters.Add("@IdCol3", idCol3);
            insertQuery.Parameters.Add("@Id1", id1);
            insertQuery.Parameters.Add("@Id2", id2);
            insertQuery.Parameters.Add("@Id3", id3);
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

            //int pSize = 0;
            LinkedList<int> ids = new LinkedList<int>();
            LinkedList<String> names = new LinkedList<String>();
            LinkedList<int> nhsNos = new LinkedList<int>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT patientID, name, nhsNo FROM PatientInformation";
            selectQuery.Parameters.Clear();
            SqlCeDataReader reader = selectQuery.ExecuteReader();

            while (reader.Read())
            {
                //pSize = pSize + 1;
                ids.AddLast(reader.GetInt32(0));
                names.AddLast(reader.GetString(1));
                nhsNos.AddLast(reader.GetInt32(2));
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

            //int cSize = 0;
            LinkedList<int> ids = new LinkedList<int>();
            LinkedList<String> conditions = new LinkedList<String>();
            LinkedList<String> descriptions = new LinkedList<String>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM Condition";
            selectQuery.Parameters.Clear();
            SqlCeDataReader reader = selectQuery.ExecuteReader();

            while (reader.Read())
            {
                //cSize = cSize + 1;
                ids.AddLast(reader.GetInt32(0));
                conditions.AddLast(reader.GetString(1));
                descriptions.AddLast(reader.GetString(2));
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

        //generalised method for all table data
        public String[] SelectStringTableData(String tableName, String column, int value)
        {
            String[] tableData = new String[2];

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
                tableData[0] = reader.GetString(1);
                tableData[1] = reader.GetString(2);
            }
            Console.WriteLine(tableData[0] + tableData[1]);

            reader.Close();

            return tableData;
        }

        //generalised method for getting ids
        public int SelectID(String columnNeeded, String tableName, String column, int value)
        {
            int id = 0;

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT @ColumnNeeded FROM @TableName WHERE @Column LIKE @Value";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@ColumnNeeded", columnNeeded);
            selectQuery.Parameters.Add("@TableName", tableName);
            selectQuery.Parameters.Add("@Column", column);
            selectQuery.Parameters.Add("@Value", value);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(1);
            }
            Console.WriteLine(id);

            reader.Close();

            return id;
        }
    }
}
