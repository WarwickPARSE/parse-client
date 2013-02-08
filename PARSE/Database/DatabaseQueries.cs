using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace PARSE.Database
{
    class DatabaseQueries
    {

        static void Main()
        {
            //
            // The name we are trying to match.
            //
            string patientName = "Bernie";
            //
            // Use preset string for connection and open it.
            //
            string connectionString = Patients.Properties.Settings.Default.ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //
                // Description of SQL command:
                // 1. It selects all cells from rows matching the name.
                // 2. It uses LIKE operator because Name is a Text field.
                // 3. @Name must be added as a new SqlParameter.
                //
                using (SqlCommand command = new SqlCommand("SELECT * FROM PatientInformation WHERE name LIKE @Name", connection))
                {
                    //
                    // Add new SqlParameter to the command.
                    //
                    command.Parameters.Add(new SqlParameter("Name", patientName));
                    //
                    // Read in the SELECT results.
                    //
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int patientID = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        string dateOfBirth = reader.GetString(2);
                        string nationality = reader.GetString(3);
                        int nhsNo = reader.GetInt32(4);
                        string address = reader.GetString(5);
                        Console.WriteLine("PatientID = {0}, Name = {1}, DateOfBirth = {2}, Nationality = {3}, NhsNo = {4}, Address = {5}", patientID, name, dateOfBirth, nationality, nhsNo, address);
                    }
                }
            }
        }
    }
}
