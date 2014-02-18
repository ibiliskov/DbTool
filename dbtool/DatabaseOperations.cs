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

        public void TestDatabases()
        {
            foreach (var database in _options.Databases)
            {
                ExecuteScalar("USE " + database);
                ConsoleHelper.WriteSuccess(database + " OK");
            }
        }

        public void Backup(string tagName)
        {
            Console.WriteLine("Starting database backup");
            for (var i = 0; i < _options.Databases.Count; i++)
            {
                var dbToRestore = _options.Databases[i];
                var dbPath = _tagging.GetLocationForDatabase(_options.Databases[i], tagName);

                Console.WriteLine("Backup {0} to {1}", dbToRestore, dbPath);
                ExecuteScalar(string.Format(
                    @"BACKUP DATABASE {0} TO  DISK = N'{1}' WITH NOFORMAT, NOINIT,  NAME = N'{0}-Full Database Backup', SKIP",
                    dbToRestore, dbPath));
            }
            ConsoleHelper.WriteSuccess("Database backup done.");
        }

        public void Restore(string tagName)
        {
            Console.WriteLine("Starting database restore");
            for (var i = 0; i < _options.Databases.Count; i++)
            {
                var dbToRestore = _options.Databases[i];
                var dbPath = _tagging.GetLocationForDatabase(_options.Databases[i], tagName);
                
                Console.WriteLine("Restoring {0} from backup {1}", dbToRestore, dbPath);
                
                var dbExists = ExecuteScalar(string.Format("IF EXISTS (SELECT * from sys.databases WHERE Name = '{0}') SELECT 1; ELSE SELECT 0;", dbToRestore));
                if ((int) dbExists == 1)
                {
                    ExecuteScalar(string.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;",
                        dbToRestore));
                }

                ExecuteScalar(string.Format(@"USE [master];RESTORE DATABASE {0} FROM  DISK = N'{1}'", dbToRestore, dbPath));
                ExecuteScalar(string.Format("ALTER DATABASE {0} SET MULTI_USER;", dbToRestore));
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
