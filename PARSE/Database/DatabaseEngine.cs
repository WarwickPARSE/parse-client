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

        public void insertScans(int scanID, int scanTypeID, String pointCloudFileReference, DateTime timestamp)
        {
            dbOpen();

            insertQueries.scans(scanID, scanTypeID, pointCloudFileReference, timestamp);

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

        public void insertPatientScans(int patientScanID, int patientID, int scanID)
        {
            dbOpen();

            insertQueries.type3("PatientScans", "patientScanID", "patientID", "scanID", patientScanID, patientID, scanID);

            dbClose();
        }

        public void insertPointRecognitionScans(int pointRecognitionScanID, int patientID, int scanLocID)
        {
            dbOpen();

            insertQueries.type3("PointRecognitionScans", "pointRecognitionScanID", "patientID", "scanLocID", pointRecognitionScanID, patientID, scanLocID);

            dbClose();
        }


        //Select methods


        // 1) select all patients (patientID and name)
        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<int>> getAllPatients()
        {
            Tuple<LinkedList<int>, LinkedList<String>, LinkedList<int>> patients = selectQueries.AllPatients();

            return patients;
        }

        // 2) select patient information
        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<DateTime>, LinkedList<String>, LinkedList<int>, LinkedList<String>> getPatientInformation(int patientID)
        {
            Tuple<LinkedList<int>, LinkedList<String>, LinkedList<DateTime>, LinkedList<String>, LinkedList<int>, LinkedList<String>> patientInfo = selectQueries.PatientInformation("patientID", patientID.ToString());

            return patientInfo;
        }

        // 3) select all conditions
        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<String>> getAllConditions()
        {
            Tuple<LinkedList<int>, LinkedList<String>, LinkedList<String>> conditions = selectQueries.AllConditions();

            return conditions;
        }

        // 4) select patient condition (and information about it)
        public LinkedList<Tuple<int, String, String>> getPatientConditionInformation(int patientID)
        {
            LinkedList<int> conditionID = selectQueries.selectID("conditionID", "PatientCondition", "patientID", patientID.ToString());

            LinkedList<Tuple<int, String, String>> patientConditions = new LinkedList<Tuple<int, String, String>>();
            while (conditionID.Count > 0)
            {
                patientConditions.AddLast(selectQueries.selectType2("Conditions", "conditionID", conditionID.First().ToString()));
                conditionID.RemoveFirst();
            }

            return patientConditions;
        }

        // 5) select condition information

        // 6) select all scan types for patient
        public void getPatientScanTypes(int patientID)
        {
            
        }

        // 7) select all timestamps for patient scan type

        // 8) select patient scans (and value)

        // 9) select all timestamps for patient point recognition scans

        // 10) select patient point recognition scans (and location)
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

        public void scans(int scanID, int scanTypeID, String pointCloudFileReference, DateTime timestamp)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO ScanLocations ((scanID, scanTypeID, pointCloudFileReference, timestamp) VALUES (@ScanID, @ScanTypeID, @pointCloudFileReference, @Timestamp)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@ScanID", scanID);
            insertQuery.Parameters.Add("@ScanTypeID", scanTypeID);
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

        //selecting all patients
        public Tuple<LinkedList<int>,LinkedList<String>,LinkedList<int>> AllPatients()
        {
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

            return Tuple.Create(ids, names, nhsNos);
        }

        //selecting all conditions
        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<String>> AllConditions()
        {
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

            return Tuple.Create(ids, conditions, descriptions);
        }

        //table type 1 queries

        //patient information
        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<DateTime>, LinkedList<String>, LinkedList<int>, LinkedList<String>> PatientInformation(String colName, String criterion)
        {
            LinkedList<int> patientID = new LinkedList<int>();
            LinkedList<String> name = new LinkedList<String>();
            LinkedList<DateTime> dob = new LinkedList<DateTime>();
            LinkedList<String> nationality = new LinkedList<string>();
            LinkedList<int> nhsNo = new LinkedList<int>();
            LinkedList<String> address = new LinkedList<string>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM PatientInformation WHERE @ColName LIKE @Criterion";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@ColName", colName);
            selectQuery.Parameters.Add("@Criterion", criterion);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                patientID.AddLast(reader.GetInt32(0));
                name.AddLast(reader.GetString(1));
                dob.AddLast(Convert.ToDateTime(reader.GetDateTime(2).ToString()));
                nationality.AddLast(reader.GetString(3));
                nhsNo.AddLast(reader.GetInt32(4));
                address.AddLast(reader.GetString(5));
            }
            reader.Close();

            return Tuple.Create(patientID, name, dob, nationality, nhsNo, address);
        }

        //scan location
        public Tuple<LinkedList<String>, LinkedList<String>, LinkedList<String>, LinkedList<double>, LinkedList<double>, LinkedList<double>, LinkedList<DateTime>> ScanLocations(String colName, String criterion)
        {
            LinkedList<int> scanLocID = new LinkedList<int>();
            LinkedList<String> boneName = new LinkedList<String>();
            LinkedList<String> jointName1 = new LinkedList<String>();
            LinkedList<String> jointName2 = new LinkedList<String>();
            LinkedList<double> distJoint1 = new LinkedList<double>();
            LinkedList<double> distJoint2 = new LinkedList<double>();
            LinkedList<double> jointsDist = new LinkedList<double>();
            LinkedList<DateTime> timestamp = new LinkedList<DateTime>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM ScanLocations WHERE @ColName LIKE @Criterion";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@ColName", colName);
            selectQuery.Parameters.Add("@Criterion", criterion);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                scanLocID.AddLast(reader.GetInt32(0));
                boneName.AddLast(reader.GetString(1));
                jointName1.AddLast(reader.GetString(2));
                jointName2.AddLast(reader.GetString(3));
                distJoint1.AddLast(reader.GetDouble(4));
                distJoint2.AddLast(reader.GetDouble(5));
                jointsDist.AddLast(reader.GetDouble(6));
                timestamp.AddLast(Convert.ToDateTime(reader.GetDateTime(7).ToString()));
            }
            reader.Close();

            //scanLocID not returned (to keep it under 8)
            return Tuple.Create(boneName, jointName1, jointName2, distJoint1, distJoint2, jointsDist, timestamp);
        }

        //scans
        public Tuple<LinkedList<int>, LinkedList<int>, LinkedList<String>, LinkedList<DateTime>> Scans(String colName, String criterion)
        {
            LinkedList<int> scanID = new LinkedList<int>();
            LinkedList<int> scanTypeID = new LinkedList<int>();
            LinkedList<String> pointCloudFileReference = new LinkedList<String>();
            LinkedList<DateTime> timestamp = new LinkedList<DateTime>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM Scans WHERE @ColName LIKE @Criterion";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@ColName", colName);
            selectQuery.Parameters.Add("@Criterion", criterion);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                scanID.AddLast(reader.GetInt32(0));
                scanTypeID.AddLast(reader.GetInt32(1));
                pointCloudFileReference.AddLast(reader.GetString(2));
                timestamp.AddLast(Convert.ToDateTime(reader.GetDateTime(3).ToString()));
            }
            reader.Close();

            return Tuple.Create(scanID, scanTypeID, pointCloudFileReference, timestamp);
        }

        //records
        public Tuple<LinkedList<int>, LinkedList<int>, LinkedList<int>, LinkedList<double>> Records(String colName, String criterion)
        {
            LinkedList<int> recordID = new LinkedList<int>();
            LinkedList<int> scanID = new LinkedList<int>();
            LinkedList<int> scanTypeID = new LinkedList<int>();
            LinkedList<double> value = new LinkedList<double>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM Records WHERE @ColName LIKE @Criterion";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@ColName", colName);
            selectQuery.Parameters.Add("@Criterion", criterion);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                recordID.AddLast(reader.GetInt32(0));
                scanID.AddLast(reader.GetInt32(1));
                scanTypeID.AddLast(reader.GetInt32(2));
                value.AddLast(reader.GetDouble(3));
            }
            reader.Close();

            return Tuple.Create(recordID, scanID, scanTypeID, value);
        }


        //table type 2 queries

        //conditions, scanTypes
        public Tuple<int, String, String> selectType2(String tableName, String colName, String criterion)
        {
            int id = 0;
            String text1 = "default";
            String text2 = "default";

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM @TableName WHERE @ColName LIKE @Criterion";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@TableName", tableName);
            selectQuery.Parameters.Add("@ColName", colName);
            selectQuery.Parameters.Add("@Criterion", criterion);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
                text1 = reader.GetString(1);
                text2 = reader.GetString(2);
            }
            reader.Close();

            return Tuple.Create(id, text1, text2);
        }


        //table type 3 queries

        //patientCondition, patientScans, pointRecognitionScans
        public Tuple<LinkedList<int>, LinkedList<int>, LinkedList<int>> selectType3(String tableName, String colName, String criterion)
        {
            LinkedList<int> id1 = new LinkedList<int>();
            LinkedList<int> id2 = new LinkedList<int>();
            LinkedList<int> id3 = new LinkedList<int>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM @TableName WHERE @ColName LIKE @Criterion";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@TableName", tableName);
            selectQuery.Parameters.Add("@ColName", colName);
            selectQuery.Parameters.Add("@Criterion", criterion);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                id1.AddLast(reader.GetInt32(0));
                id2.AddLast(reader.GetInt32(1));
                id3.AddLast(reader.GetInt32(2));
            }
            reader.Close();

            return Tuple.Create(id1, id2, id3);
        }

        //generalised method for getting one id
        public LinkedList<int> selectID(String columnNeeded, String tableName, String column, String criterion)
        {
            LinkedList<int> id = new LinkedList<int>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT @ColumnNeeded FROM @TableName WHERE @Column LIKE @Criterion";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@ColumnNeeded", columnNeeded);
            selectQuery.Parameters.Add("@TableName", tableName);
            selectQuery.Parameters.Add("@Column", column);
            selectQuery.Parameters.Add("@Criterion", criterion);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                id.AddLast(reader.GetInt32(0));
            }

            reader.Close();

            return id;
        }
    }
}
