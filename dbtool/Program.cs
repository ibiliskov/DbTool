using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

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

            try
            {
                if (!Directory.Exists(_options.BackupFolder))
                    throw new ArgumentException("Backup foler doesn't exist. Please create it.");

                _tagging = new Tagging(_options);

                ParseCommand(args);
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError(ex.Message);
            }
        }

        static void ParseCommand(string[] args)
        {
            if (args.Length == 0)
            {
                EchoHelp();
                return;
            }

            var db = new DatabaseOperations(_options, _tagging);
            var tags = _tagging.GetTagList();

            try
            {
                switch (args[0])
                {
                    case "test":
                        db.TestConnection();
                        ConsoleHelper.WriteSuccess("Connection OK");
                        break;
                    case "save":
                        if (args.Length < 2)
                        {
                            EchoHelp();
                            return;
                        }

                        var tagName = args[1];
                        if (args[1] == "now")
                            tagName = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");

                        db.Backup(tagName);
                        break;
                    case "load":
                        if (args.Length < 2)
                        {
                            EchoHelp();
                            return;
                        }

                        if (args[1] == "last")
                            args[1] = "0)";

                        if (args[1].EndsWith(")") && args[1].Length > 1)
                        {
                            var tag = _tagging.GetTagAtPosition(int.Parse(args[1].Remove(args[1].Length - 1)));
                            db.Restore(tag);
                        }
                        else
                        {
                            if (tags.Contains(args[1]))
                                db.Restore(args[1]);
                            else
                            {
                                
                                var filteredTags = _tagging.GetTagList(args[1]);
                                if (filteredTags.Count > 0)
                                {
                                    Console.WriteLine("Select backup number you want to restore, N to cancel:");
                                    for (var i = 0; i < filteredTags.Count; i++)
                                        Console.WriteLine(" {0}) {1}", i, filteredTags[i]);

                                    var userInput = Console.ReadLine();
                                    if (userInput != null && userInput.ToLower() != "n")
                                    {
                                        var position = -1;
                                        if (userInput.Length > 1 && userInput.EndsWith(")"))
                                        {
                                            position = int.Parse(userInput.Remove(userInput.Length - 1));
                                        }
                                        else
                                        {
                                            position = int.Parse(userInput);
                                        }

                                        if (position > -1)
                                        {
                                            var tag = _tagging.GetTagAtPosition(position, args[1]);
                                            db.Restore(tag);
                                        }
                                    }
                                }
                                else throw new ArgumentException("Cannot load non-existing tag");
                            }
                        }
                        break;
                    case "list":
                        for (var i = 0; i < tags.Count; i++)
                            Console.WriteLine(" {0}) {1}", i, tags[i]);
                        break;
                    case "delete":
                        if (args.Length < 2)
                        {
                            EchoHelp();
                            return;
                        }

                        if (args[1].EndsWith(")") && args[1].Length > 1)
                        {
                            _tagging.DeleteAtPosition(int.Parse(args[1].Remove(args[1].Length - 1)));
                        }
                        else
                        {
                            _tagging.Delete(args[1]);
                        }
                        break;
                    case "drop":
                        ConsoleHelper.WriteWarning("You want to drop your databases? (Y/N)");
                        var pressed = Console.ReadLine();
                        Console.WriteLine();
                        if (pressed != null && pressed.ToLower() == "y")
                        {
                            db.Drop();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError(ex.Message);
            }
        }

        static void EchoHelp()
        {
            Console.WriteLine("USAGE: ");
            Console.WriteLine("    list                         list all created tags");
            Console.WriteLine("    save <tag> | now             create database backup to specified folder");
            Console.WriteLine("    load <tag> | last | <num>)   restore database backup from specific tag or number");
            Console.WriteLine("    delete <tag> | <num>)        delete database backup by tag name or number");
            Console.WriteLine("    drop                         deletes database from server");
        }
    }
}
