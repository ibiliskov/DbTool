using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace dbtool
{
    public class DatabaseOperations
    {
        private readonly Options _options;

        public DatabaseOperations(Options options)
        {
            _options = options;
        }

        public void TestConnection()
        {
            const int test = 3;
            var connection = new SqlConnection(_options.ConnectionString);
            var command = new SqlCommand("SELECT " + test, connection);
            connection.Open();

            var result = command.ExecuteScalar();

            connection.Close();

            if ((int) result != test)
                throw new ArgumentException("Cannot connect to database.");
        }

        public void Backup()
        {
            
        }

        public void Restore()
        {
            
        }
    }
}
