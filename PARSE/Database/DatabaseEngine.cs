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

        public Insertion insertQueries;
        public Selection selectQueries;

        public DatabaseEngine()
        {
            this.insertQueries = new Insertion(this.con);
            this.selectQueries = new Selection(this.con);
        }

        public void dbOpen()
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

        public void dbClose()
        {
            con.Close();

            Console.WriteLine("closed");
        }

        //Insert methods for all tables

        //Type 1 = tables that need a specialised query
        public void insertPatientInformation(String name, String dateOfBirth, String nationality, int nhsNo, String address, String weight)
        {
            dbOpen();

            Insertion ins = new Insertion(con);

            ins.patientInformation(name, dateOfBirth, nationality, nhsNo, address, weight);

            dbClose();
        }

        public void insertScanLocations(String boneName, String jointName1, String jointName2, double distJoint1, double distJoint2, String jointsDist, DateTime timestamp)
        {
            dbOpen();

            insertQueries.scanLocations(boneName, jointName1, jointName2, distJoint1, distJoint2, jointsDist, timestamp);

            dbClose();
        }

        public void insertScans(int scanTypeID, int patientID, String pointCloudFileReference, String description, DateTime timestamp)
        {
            dbOpen();

            Insertion ins = new Insertion(con);

            ins.scans(scanTypeID, pointCloudFileReference, description, timestamp);
            ins.patientscans(patientID);

            dbClose();
        }

        public void insertRecords(int scanID, int scanTypeID, double value)
        {
            dbOpen();

            insertQueries.records(scanID, scanTypeID, value);

            dbClose();
        }

        public void insertLimbCoordinates(int scanID, double[] joint1, double[] joint2, String[] joint3, String[] joint4)
        {
            dbOpen();

            insertQueries.LimbCoordinates(scanID, joint1, joint2, joint3, joint4);

            dbClose();
        }

        //Type 2 = tables containing 1 identifier and 2 text fields

        //contains the different types of conditions
        public void insertConditions(String condition, String description)
        {
            dbOpen();

            insertQueries.type2("Conditions", "condition", "description", condition, description);

            dbClose();
        }

        //contains the different types of scans
        public void insertScanTypes(String scanType, String description)
        {
            dbOpen();

            insertQueries.type2("ScanTypes", "scanType", "description", scanType, description);

            dbClose();
        }

        //Type 3 = tables containing 3 identifiers

        public void insertPatientCondition(int patientID, int conditionID)
        {
            dbOpen();

            insertQueries.type3("PatientCondition", "patientID", "conditionID", patientID, conditionID);

            dbClose();
        }

        public void insertPatientScans(int patientID, int scanID)
        {
            dbOpen();

            insertQueries.type3("PatientScans", "patientID", "scanID", patientID, scanID);

            dbClose();
        }

        public void insertPointRecognitionScans(int patientID, int scanLocID)
        {
            dbOpen();

            insertQueries.type3("PointRecognitionScans", "patientID", "scanLocID", patientID, scanLocID);

            dbClose();
        }


        //Select methods


        // 1) select all patients (patientID and name)
        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<String>> getAllPatients()
        {
            dbOpen();
            Tuple<LinkedList<int>, LinkedList<String>, LinkedList<String>> patients = null;
            Selection sr = new Selection(con);

            try
            {
                patients = sr.AllPatients();
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine("Error is occuring here");
            }

            dbClose();

            return patients;
        }

        // 2) select patient information
        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<DateTime>, LinkedList<String>, LinkedList<String>, LinkedList<String>, LinkedList<String>> getPatientInformation(int patientID)
        {

            Selection sr = new Selection(con);

            Tuple<LinkedList<int>, LinkedList<String>, LinkedList<DateTime>, LinkedList<String>, LinkedList<String>, LinkedList<String>, LinkedList<String>> patientInfo = sr.PatientInformation("patientID", patientID.ToString());

            return patientInfo;
        }

        // 3) select all conditions
        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<String>> getAllConditions()
        {
            Tuple<LinkedList<int>, LinkedList<String>, LinkedList<String>> conditions = selectQueries.AllConditions();

            return conditions;
        }

        // 4) select patient condition (and information about it)
        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<String>> getPatientConditionInformation(int patientID)
        {
            Tuple<LinkedList<int>, LinkedList<String>, LinkedList<String>> patientConditions = selectQueries.patientConditions(patientID);

            return patientConditions;
        }

        // 5) select limb coordinates
        public Tuple<LinkedList<int>, LinkedList<double[]>, LinkedList<double[]>, LinkedList<String[]>, LinkedList<String[]>> getLimbCoordinates(int scanID)
        {
           /* Tuple<LinkedList<int>, LinkedList<double[]>, LinkedList<double[]>, LinkedList<double[]>, LinkedList<double[]>> limbCoords = selectQueries.limbCoordinates(scanID);

            return limbCoords;*/

            return null;
        }

        // 6) select all scan types for patient
        public Tuple<LinkedList<int>, LinkedList<String>> getPatientScanTypes(int patientID)
        {
            Tuple<LinkedList<int>, LinkedList<String>> patientScanTypes = selectQueries.scanTypesForPatient(patientID);

            return patientScanTypes;
        }

        // 7) select all timestamps for patient scan type
        public Tuple<LinkedList<int>, LinkedList<DateTime>> getScanTimestamps(int scanTypeID)
        {
            Tuple<LinkedList<int>, LinkedList<DateTime>> timestamps = selectQueries.timestampsForPatient(scanTypeID);

            return timestamps;
        }

      
        // 7a) gets scan IDs from patientScans table
        public Tuple<LinkedList<int>, LinkedList<int>> getScanIDs(int patientID)
        {
            Tuple<LinkedList<int>, LinkedList<int>> scanIDs = selectQueries.selectType3("PatientScans", "patientID", patientID.ToString());

            return scanIDs;
        }

        // 7b) select all timestamps and scan IDs for patient scans
        public Tuple<LinkedList<int>, LinkedList<DateTime>> timestampsForPatientScans(int patientID)
        {

            Tuple<LinkedList<int>, LinkedList<DateTime>> timestamps = selectQueries.timestampsForPatientScans(patientID);
            return timestamps;
        }

        // 8) select patient scans (and value)
        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<DateTime>, LinkedList<double>> getScanResult(int ScanID)
        {
            Tuple<LinkedList<int>, LinkedList<String>, LinkedList<DateTime>, LinkedList<double>> result = selectQueries.scanValueForPatient(ScanID);

            return result;
        }

        // 9) select all timestamps for patient point recognition scans
        public Tuple<LinkedList<int>, LinkedList<DateTime>> timestampsForPatientLocScans(int patientID)
        {
            Tuple<LinkedList<int>, LinkedList<DateTime>> timestamps = selectQueries.timestampsForPatientLocScans(patientID);

            return timestamps;
        }

        // 10) select patient point recognition scans (and location)
        public Tuple<LinkedList<String>, LinkedList<String>, LinkedList<String>, LinkedList<double>, LinkedList<double>, LinkedList<double>, LinkedList<DateTime>> getScanLocation(int scanLocID)
        {
            Tuple<LinkedList<String>, LinkedList<String>, LinkedList<String>, LinkedList<double>, LinkedList<double>, LinkedList<double>, LinkedList<DateTime>> locations = selectQueries.ScanLocations("patientID", scanLocID.ToString());

            return locations;
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

        public void patientInformation(String name, String dateOfBirth, String nationality, int nhsNo, String address, String weight)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO PatientInformation (name, dateofbirth, nationality, nhsNo, address, weight) VALUES (@Name, @DateOfBirth, @Nationality, @NhsNo, @Address, @Weight)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@Name", name);
            insertQuery.Parameters.Add("@DateOfBirth", Convert.ToDateTime(dateOfBirth));
            insertQuery.Parameters.Add("@Nationality", nationality);
            insertQuery.Parameters.Add("@NhsNo", nhsNo);
            insertQuery.Parameters.Add("@Address", address);
            insertQuery.Parameters.Add("@Weight", weight);
            rowsAffected = insertQuery.ExecuteNonQuery();

            System.Diagnostics.Debug.WriteLine("Rows affected: " + rowsAffected);

        }

        public void scanLocations(String boneName, String jointName1, String jointName2, double distJoint1, double distJoint2, String jointsDist, DateTime timestamp)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO ScanLocations ((boneName, jointName1, jointName2, distJoint1, distJoint2, jointsDist, timestamp) VALUES (@BoneName, @JointName1, @JointName2, @DistJoint1, @DistJoint2, @JointsDist, @Timestamp)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@BoneName", boneName);
            insertQuery.Parameters.Add("@JointName1", jointName1);
            insertQuery.Parameters.Add("@JointName2", jointName2);
            insertQuery.Parameters.Add("@DistJoint1", distJoint1);
            insertQuery.Parameters.Add("@DistJoint2", distJoint2);
            insertQuery.Parameters.Add("@JointsDist", jointsDist);
            insertQuery.Parameters.Add("@Timestamp", timestamp.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            rowsAffected = insertQuery.ExecuteNonQuery();
        }

        public void scans(int scanTypeID, String pointCloudFileReference, String description, DateTime timestamp)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO Scans (scanTypeID, pointCloudFileReference, description, timestamp) VALUES (@ScanTypeID, @PointCloudFileReference, @Description, @Timestamp)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@ScanTypeID", scanTypeID);
            insertQuery.Parameters.Add("@PointCloudFileReference", pointCloudFileReference);
            insertQuery.Parameters.Add("@Description", description);
            insertQuery.Parameters.Add("@Timestamp", timestamp.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            rowsAffected = insertQuery.ExecuteNonQuery();

            System.Diagnostics.Debug.WriteLine(rowsAffected);
        }

        public void patientscans(int patientID)
        {
            int rowsAffected = 0;
            int scanId = 0;

            //select query for getting last added scan

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT MAX(scanID) FROM Scans";
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                scanId = reader.GetInt32(0);
            }
            reader.Close();

            //insert query for adding latest scan to the patientscans table
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO PatientScans (patientID, scanID) VALUES (@patientID, @scanID)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@patientID", patientID);
            insertQuery.Parameters.Add("@scanID", scanId);
            rowsAffected = insertQuery.ExecuteNonQuery();
            
                  
        }

        public void records(int scanID, int scanTypeID, double value)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO Records (scanID, scanTypeID, value) VALUES (@ScanID, @ScanTypeID, @Value)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@ScanID", scanID);
            insertQuery.Parameters.Add("@ScanTypeID", scanTypeID);
            insertQuery.Parameters.Add("@Value", value);
            rowsAffected = insertQuery.ExecuteNonQuery();
        }

        public void LimbCoordinates(int scanID, double[] joint1, double[] joint2, String[] joint3, String[] joint4)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO LimbCoordinates (scanID, joint1x, joint1y, joint1z, joint2x, joint2y, joint2z, joint3x, joint3y, joint3z, joint4x, joint4y, joint4z) VALUES (@ScanID, @Joint1x, @Joint1y, @Joint1z, @Joint2x, @Joint2y, @Joint2z, @Joint3x, @Joint3y, @Joint3z, @Joint4x, @Joint4y, @Joint4z)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@ScanID", scanID);
            insertQuery.Parameters.Add("@Joint1x", joint1[0]);
            insertQuery.Parameters.Add("@Joint1y", joint1[1]);
            insertQuery.Parameters.Add("@Joint1z", joint1[2]);
            insertQuery.Parameters.Add("@Joint2x", joint2[0]);
            insertQuery.Parameters.Add("@Joint2y", joint2[1]);
            insertQuery.Parameters.Add("@Joint2z", joint2[2]);
            insertQuery.Parameters.Add("@Joint3x", joint3[0]);
            insertQuery.Parameters.Add("@Joint3y", joint3[1]);
            insertQuery.Parameters.Add("@Joint3z", joint3[2]);
            insertQuery.Parameters.Add("@Joint4x", joint4[0]);
            insertQuery.Parameters.Add("@Joint4y", joint4[1]);
            insertQuery.Parameters.Add("@Joint4z", joint4[2]);
            rowsAffected = insertQuery.ExecuteNonQuery();
        }

        //Insertion Query for Type 2 tables

        public void type2(String tableName, String textCol1, String textCol2, String text1, String text2)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO @TableName (@TextCol1, @TextCol2) VALUES ('@Text1', '@Text2')";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@TableName", tableName);
            insertQuery.Parameters.Add("@TextCol1", textCol1);
            insertQuery.Parameters.Add("@TextCol2", textCol2);
            insertQuery.Parameters.Add("@Text1", text1);
            insertQuery.Parameters.Add("@Text2", text2);
            rowsAffected = insertQuery.ExecuteNonQuery();
        }
        //Insertion Query for Type 3 tables

        public void type3(String tableName, String idCol1, String idCol2, int id1, int id2)
        {
            int rowsAffected = 0;
            SqlCeCommand insertQuery = this.con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO @TableName (@IdCol1, @IdCol2) VALUES (@Id1, @Id2)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@TableName", tableName);
            insertQuery.Parameters.Add("@IdCol1", idCol1);
            insertQuery.Parameters.Add("@IdCol2", idCol2);
            insertQuery.Parameters.Add("@Id1", id1);
            insertQuery.Parameters.Add("@Id2", id2);
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
        public Tuple<LinkedList<int>,LinkedList<String>,LinkedList<String>> AllPatients()
        {

            LinkedList<int> ids = new LinkedList<int>();
            LinkedList<String> names = new LinkedList<String>();
            LinkedList<String> nhsNos = new LinkedList<String>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT patientID, name, nhsNo FROM PatientInformation";
            selectQuery.Parameters.Clear();
            SqlCeDataReader reader = selectQuery.ExecuteReader();

            while (reader.Read())
            {
                ids.AddLast(reader.GetInt32(0));
                names.AddLast(reader.GetString(1));
                nhsNos.AddLast(reader.GetString(2));
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
        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<DateTime>, LinkedList<String>, LinkedList<String>, LinkedList<String>, LinkedList<String>> PatientInformation(String colName, String criterion)
        {
            LinkedList<int> patientID = new LinkedList<int>();
            LinkedList<String> name = new LinkedList<String>();
            LinkedList<DateTime> dob = new LinkedList<DateTime>();
            LinkedList<String> nationality = new LinkedList<String>();
            LinkedList<String> nhsNo = new LinkedList<String>();
            LinkedList<String> address = new LinkedList<String>();
            LinkedList<String> weight = new LinkedList<String>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM PatientInformation WHERE patientID = " + criterion;
            selectQuery.Parameters.Clear();
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                patientID.AddLast(reader.GetInt32(0));
                name.AddLast(reader.GetString(1));
                dob.AddLast(Convert.ToDateTime(reader.GetDateTime(2).ToString()));
                nationality.AddLast(reader.GetString(3));
                nhsNo.AddLast(reader.GetString(4));
                address.AddLast(reader.GetString(5));
                weight.AddLast(reader.GetString(6));
            }
            reader.Close();

            return Tuple.Create(patientID, name, dob, nationality, nhsNo, address, weight);
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
                String b;
                try
                {
                    b = reader.GetString(1);
                } catch(Exception e) {
                    b = "null";
                }
                boneName.AddLast(b);
                jointName1.AddLast(reader.GetString(2));
                jointName2.AddLast(reader.GetString(3));
                distJoint1.AddLast(reader.GetDouble(4));
                distJoint2.AddLast(reader.GetDouble(5));
                double d;
                try
                {
                    d = reader.GetDouble(6);
                }
                catch (Exception e)
                {
                    d = 0;
                }
                jointsDist.AddLast(d);
                timestamp.AddLast(Convert.ToDateTime(reader.GetDateTime(7).ToString()));
            }
            reader.Close();

            //scanLocID not returned (to keep it under 8) !!!
            return Tuple.Create(boneName, jointName1, jointName2, distJoint1, distJoint2, jointsDist, timestamp);
        }

        //scans
        public Tuple<LinkedList<int>, LinkedList<int>, LinkedList<String>, LinkedList<String>, LinkedList<DateTime>> Scans(String colName, String criterion)
        {
            LinkedList<int> scanID = new LinkedList<int>();
            LinkedList<int> scanTypeID = new LinkedList<int>();
            LinkedList<String> pointCloudFileReference = new LinkedList<String>();
            LinkedList<String> description = new LinkedList<String>();
            LinkedList<DateTime> timestamp = new LinkedList<DateTime>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM Scans WHERE scanID = " + criterion;
            selectQuery.Parameters.Clear();
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                scanID.AddLast(reader.GetInt32(0));
                scanTypeID.AddLast(reader.GetInt32(1));
                pointCloudFileReference.AddLast(reader.GetString(2));
                description.AddLast(reader.GetString(3));
                timestamp.AddLast(Convert.ToDateTime(reader.GetDateTime(4).ToString()));
            }
            reader.Close();

            return Tuple.Create(scanID, scanTypeID, pointCloudFileReference, description, timestamp);
        }

        //records
        public Tuple<LinkedList<int>, LinkedList<int>, LinkedList<double>> Records(String colName, String criterion)
        {
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
                scanID.AddLast(reader.GetInt32(0));
                scanTypeID.AddLast(reader.GetInt32(1));
                value.AddLast(reader.GetDouble(2));
            }
            reader.Close();

            return Tuple.Create(scanID, scanTypeID, value);
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
        public Tuple<LinkedList<int>, LinkedList<int>> selectType3(String tableName, String colName, String criterion)
        {
            LinkedList<int> id1 = new LinkedList<int>();
            LinkedList<int> id2 = new LinkedList<int>();

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
            }
            reader.Close();

            return Tuple.Create(id1, id2);
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

        public Tuple<LinkedList<int>, LinkedList<double[]>, LinkedList<double[]>, LinkedList<String[]>, LinkedList<String[]>> limbCoordinates(int ScanID)
        {
            LinkedList<int> scanID = new LinkedList<int>();

            double[] d = new double[3];
            LinkedList<double[]> joint1 = new LinkedList<double[]>();
            LinkedList<double[]> joint2 = new LinkedList<double[]>();

            String[] s = new String[3];
            LinkedList<String[]> joint3 = new LinkedList<String[]>();
            LinkedList<String[]> joint4 = new LinkedList<String[]>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "Select * from limbCoordinates where scanID = @ScanID";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@ScanID", ScanID);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                scanID.AddLast(reader.GetInt32(0));
                d[0] = reader.GetDouble(1);
                d[1] = reader.GetDouble(2);
                d[2] = reader.GetDouble(3);
                joint1.AddLast(d);
                d[0] = reader.GetDouble(4);
                d[1] = reader.GetDouble(5);
                d[2] = reader.GetDouble(6);
                joint2.AddLast(d);
                System.Type type = reader.GetFieldType(4);

                if (Type.GetTypeCode(type) == TypeCode.String)
                {
                    s[0] = reader.GetString(7);
                    s[1] = reader.GetString(8);
                    s[2] = reader.GetString(9);
                    joint3.AddLast(s);
                }
                else if (Type.GetTypeCode(type) == TypeCode.Double)
                {
                    s[0] = reader.GetDouble(7).ToString();
                    s[1] = reader.GetDouble(8).ToString();
                    s[2] = reader.GetDouble(9).ToString();
                    joint3.AddLast(s);
                }
                type = reader.GetFieldType(10);
                if (Type.GetTypeCode(type) == TypeCode.String)
                {
                    s[0] = reader.GetString(10);
                    s[1] = reader.GetString(11);
                    s[2] = reader.GetString(12);
                    joint3.AddLast(s);
                }
                else if (Type.GetTypeCode(type) == TypeCode.Double)
                {
                    s[0] = reader.GetDouble(10).ToString();
                    s[1] = reader.GetDouble(11).ToString();
                    s[2] = reader.GetDouble(12).ToString();
                    joint3.AddLast(s);
                }
            }
            reader.Close();

            return Tuple.Create(scanID, joint1, joint2, joint3, joint4);
        }

        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<String>> patientConditions(int patientID)
        {
            LinkedList<int> conditionID = new LinkedList<int>();
            LinkedList<String> condition = new LinkedList<String>();
            LinkedList<String> description = new LinkedList<String>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "Select PatientCondition.ConditionID, condition, description from PatientCondition join Conditions on PatientCondition.conditionID = Conditions.conditionID where patientID = @PatientID";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@PatientID", patientID);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                conditionID.AddLast(reader.GetInt32(0));
                condition.AddLast(reader.GetString(1));
                description.AddLast(reader.GetString(2));
            }
            reader.Close();

            return Tuple.Create(conditionID, condition, description);
        } 

        public Tuple<LinkedList<int>, LinkedList<String>> scanTypesForPatient(int patientID)
        {
            LinkedList<int> scanTypeID = new LinkedList<int>();
            LinkedList<String> scanType = new LinkedList<String>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "Select ScanTypes.scanTypeID, scanType from ScanTypes join Scans on ScanTypes.scanTypeID = Scans.scanTypeID join PatientScans on PatientScans.scanID = Scans.scanID where PatientScans.patientID = @PatientID";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@PatientID", patientID);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                scanTypeID.AddLast(reader.GetInt32(0));
                scanType.AddLast(reader.GetString(1));
            }
            reader.Close();

            return Tuple.Create(scanTypeID, scanType);
        }

        public Tuple<LinkedList<int>, LinkedList<DateTime>> timestampsForPatient(int scanTypeID)
        {
            LinkedList<int> scanID = new LinkedList<int>();
            LinkedList<DateTime> timestamp = new LinkedList<DateTime>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "Select Scans.scanID, timestamp from Scans join PatientScans on Scans.scanID = PatientScans.scanID where scanTypeID = @ScanTypeID";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@ScanTypeID", scanTypeID);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                scanID.AddLast(reader.GetInt32(0));
                timestamp.AddLast(Convert.ToDateTime(reader.GetDateTime(1).ToString()));
            }
            reader.Close();

            return Tuple.Create(scanID, timestamp);
        }

        public Tuple<LinkedList<int>, LinkedList<String>, LinkedList<DateTime>, LinkedList<double>> scanValueForPatient(int ScanID)
        {
            LinkedList<int> scanID = new LinkedList<int>();
            LinkedList<String> pointCloudFileReference = new LinkedList<String>();
            LinkedList<DateTime> timestamp = new LinkedList<DateTime>();
            LinkedList<double> value = new LinkedList<double>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "Select Scans.scanID, pointCloudFileReference, timestamp, Records.value from Scans join Records on Scans.scanID = Records.scanID where Scans.scanID = @ScanID;";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@ScanID", ScanID);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                scanID.AddLast(reader.GetInt32(0));
                pointCloudFileReference.AddLast(reader.GetString(1));
                timestamp.AddLast(Convert.ToDateTime(reader.GetDateTime(2).ToString()));
                value.AddLast((double)reader.GetFloat(3));
            }
            reader.Close();

            return Tuple.Create(scanID, pointCloudFileReference, timestamp, value);
        }

        public Tuple<LinkedList<int>, LinkedList<DateTime>> timestampsForPatientScans(int patientID)
        {
            LinkedList<int> scanID = new LinkedList<int>();
            LinkedList<DateTime> timestamp = new LinkedList<DateTime>();

            this.con = new SqlCeConnection();
            this.con.ConnectionString = "Data Source=|DataDirectory|\\Patients.sdf";
            this.con.Open();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "Select Scans.scanID, timestamp from Scans join PatientScans on Scans.scanID = PatientScans.scanID where patientID = @PatientID;";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@PatientID", patientID);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                scanID.AddLast(reader.GetInt32(0));
                System.Diagnostics.Debug.WriteLine(reader.GetInt32(0));
                timestamp.AddLast(Convert.ToDateTime(reader.GetDateTime(1).ToString()));
                System.Diagnostics.Debug.WriteLine(reader.GetDateTime(1));
            }
            reader.Close();

            return Tuple.Create(scanID, timestamp);
        }

        public Tuple<LinkedList<int>, LinkedList<DateTime>> timestampsForPatientLocScans(int patientID)
        {
            LinkedList<int> scanLocID = new LinkedList<int>();
            LinkedList<DateTime> timestamp = new LinkedList<DateTime>();

            SqlCeCommand selectQuery = this.con.CreateCommand();
            selectQuery.CommandText = "Select ScanLocations.scanLocID, timestamp from ScanLocations join PointRecognitionScans on ScanLocations.scanLocID = PointRecognitionScans.scanLocID where patientID = @PatientID;";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@PatientID", patientID);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                scanLocID.AddLast(reader.GetInt32(0));
                timestamp.AddLast(Convert.ToDateTime(reader.GetDateTime(1).ToString()));
            }
            reader.Close();

            return Tuple.Create(scanLocID, timestamp);
        }
    }
}
