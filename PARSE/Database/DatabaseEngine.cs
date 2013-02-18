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

                GetTest();

                dbClose();
        }

        private void dbOpen()
        {
            try
            {
                con = new SqlCeConnection();
                con.ConnectionString = "Data Source=|DataDirectory|\\Patients.sdf";
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

        public void AddTest()
        {
            int patientID = 3;
            string name = "Robin";
            string dateOfBirth = "15/12/1950";
            string nationality = "Random";
            int nhsNo = 3;
            string address = "RobinAddress";

            int rowsAffected = 0;
            SqlCeCommand insertQuery = con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO PatientInformation (patientID, name, nationality, nhsNo, address) VALUES (@PatientID, @Name, @Nationality, @NhsNo, @Address)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@PatientID", patientID);
            insertQuery.Parameters.Add("@Name", name);
            insertQuery.Parameters.Add("@Nationality", nationality);
            insertQuery.Parameters.Add("@NhsNo", nhsNo);
            insertQuery.Parameters.Add("@Address", address);
            rowsAffected = insertQuery.ExecuteNonQuery();
        }

        public void GetTest()
        {
            String patientName = "Bernie";

            int patientID = 0;
            string name = "default";

            string nationality = "default";
            int nhsNo = 0;
            string address = "default";

            SqlCeCommand selectQuery = con.CreateCommand();
            selectQuery.CommandText = "SELECT * FROM PatientInformation WHERE name LIKE @PatientName";
            selectQuery.Parameters.Clear();
            selectQuery.Parameters.Add("@PatientName", patientName);
            SqlCeDataReader reader = selectQuery.ExecuteReader();
            while (reader.Read())
            {
                patientID = reader.GetInt32(0);
                name = reader.GetString(1);

                nationality = reader.GetString(3);
                nhsNo = reader.GetInt32(4);
                address = reader.GetString(5);
            }
            Console.WriteLine(nationality);

            reader.Close();
        }
    }

    class Insertion
    {
        public void InsertPatientInformation(SqlCeConnection con, int patientID, String name, String dateOfBirth, String nationality, int nhsNo, String address)
        {
            //check what number is the previous patientID and increment by 1?

            int rowsAffected = 0;
            SqlCeCommand insertQuery = con.CreateCommand();
            insertQuery.CommandText = "INSERT INTO PatientInformation (patientID, name, nationality, nhsNo, address) VALUES (@PatientID, @Name, @DateOfBirth, @Nationality, @NhsNo, @Address)";
            insertQuery.Parameters.Clear();
            insertQuery.Parameters.Add("@PatientID", patientID);
            insertQuery.Parameters.Add("@Name", name);
            insertQuery.Parameters.Add("@DateOfBirth", dateOfBirth);
            insertQuery.Parameters.Add("@Nationality", nationality);
            insertQuery.Parameters.Add("@NhsNo", nhsNo);
            insertQuery.Parameters.Add("@Address", address);
            rowsAffected = insertQuery.ExecuteNonQuery();
        }
    }

    class Selection
    {
        public void SelectPatientInformation()
        {

        }
    }
}
