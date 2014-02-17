﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace dbtool
{
    class Program
    {
        private static Options _options;
        private static Tagging _tagging;

        static void Main(string[] args)
        {
            _options = new Options
            {
                BackupFolder = ConfigurationManager.AppSettings.Get("backupFolder"),
                ConnectionString = ConfigurationManager.AppSettings.Get("connectionString"),
                Databases = new List<string>(ConfigurationManager.AppSettings.Get("databases").Split(';', ','))
            };

            _tagging = new Tagging(_options);

            ParseCommand(args);
        }

        static void ParseCommand(string[] args)
        {
            if (args.Length == 0)
            {
                EchoHelp();
                return;
            }

            var db = new DatabaseOperations(_options, _tagging);

            try
            {
                switch (args[0])
                {
                    case "test":
                        db.TestConnection();
                        Console.WriteLine("Connection OK");
                        break;
                    case "save":
                        if (args.Length < 2)
                        {
                            EchoHelp();
                            return;
                        }

                        db.Backup(args[1]);
                        break;
                    case "load":
                        if (args.Length < 2)
                        {
                            EchoHelp();
                            return;
                        }

                        db.Restore(args[1]);
                        break;
                    case "list":
                        var tags = _tagging.GetTagList();
                        foreach (var tag in tags)
                            Console.WriteLine(tag);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        static void EchoHelp()
        {
            Console.WriteLine("USAGE: ");
            Console.WriteLine("    list            list all created tags");
            Console.WriteLine("    save <tag>      create database backup to specified folder");
            Console.WriteLine("    load <tag>      restore database backup from specific tag");
            Console.WriteLine("    delete <tag>    delete database backup by tag name");
            Console.WriteLine("    drop            deletes database from server");
        }
    }
}
