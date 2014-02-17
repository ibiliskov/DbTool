using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace dbtool
{
    class Program
    {
        private static Options _options = null;

        static void Main(string[] args)
        {
            _options = new Options
            {
                BackupFolder = ConfigurationManager.AppSettings.Get("backupFolder"),
                ConnectionString = ConfigurationManager.AppSettings.Get("connectionString")
            };

            ParseCommand(args);
        }

        static void ParseCommand(string[] args)
        {
            var db = new DatabaseOperations(_options);
            db.TestConnection();
        }
    }
}
