using System;
using System.Data.SqlClient;

namespace dbtool
{
    public class DatabaseOperations
    {
        private readonly Options _options;
        private readonly Tagging _tagging;

        public DatabaseOperations(Options options, Tagging tagging)
        {
            _options = options;
            _tagging = tagging;
        }

        public void TestConnection()
        {
            if ((int)ExecuteScalar("SELECT 3") != 3)
                throw new ArgumentException("Cannot connect to database.");
        }

        public void Backup(string tagName)
        {
            Console.WriteLine("Starting database backup");
            for (var i = 0; i < _options.Databases.Count; i++)
            {
                Console.WriteLine("Db: " + _options.Databases[i]);
                ExecuteScalar(string.Format(
                    @"BACKUP DATABASE {0} TO  DISK = N'{1}' WITH NOFORMAT, NOINIT,  NAME = N'{0}-Full Database Backup', SKIP",
                    _options.Databases[i],
                    _tagging.GetLocationForDatabase(_options.Databases[i], tagName)
                    ));
            }
            ConsoleHelper.WriteSuccess("Database backup done.");
        }

        public void Restore(string tagName)
        {
            Console.WriteLine("Starting database restore");
            for (var i = 0; i < _options.Databases.Count; i++)
            {
                Console.WriteLine("Db: " + _options.Databases[i]);
                ExecuteScalar(string.Format(
                    @"USE [master];RESTORE DATABASE {0} FROM  DISK = N'{1}'",
                    _options.Databases[i],
                    _tagging.GetLocationForDatabase(_options.Databases[i], tagName)
                    ));
            }
            ConsoleHelper.WriteSuccess("Database restore done.");
        }

        public void Drop()
        {
            Console.WriteLine("Dropping database...");
            for (var i = 0; i < _options.Databases.Count; i++)
            {
                Console.WriteLine("Db: " + _options.Databases[i]);
                ExecuteScalar(string.Format(@"DROP DATABASE {0}", _options.Databases[i]));
            }
            ConsoleHelper.WriteSuccess("Finished");
        }

        private object ExecuteScalar(string sqlCommand)
        {
            var connection = new SqlConnection(_options.ConnectionString);
            var command = new SqlCommand(sqlCommand, connection);
            connection.Open();

            var result = command.ExecuteScalar();

            connection.Close();

            return result;
        }
    }
}
