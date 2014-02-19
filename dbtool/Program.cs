using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace dbtool
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options
            {
                BackupFolder = ConfigurationManager.AppSettings.Get("backupFolder"),
                ConnectionString = ConfigurationManager.AppSettings.Get("connectionString"),
                Databases = new List<string>(ConfigurationManager.AppSettings.Get("databases").Split(';', ','))
            };

            try
            {
                if (!Directory.Exists(options.BackupFolder))
                    throw new ArgumentException("Backup foler doesn't exist. Edit app.config and create folder.");

                var tagging = new Tagging(options);
                var command = new Input(args);

                ProcessCommand(command, options, tagging, new DatabaseOperations(options, tagging), new Compress(tagging, options));
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError(ex.Message);
            }
        }

        static void ProcessCommand(Input input, Options options, Tagging tagging, DatabaseOperations db, Compress compress)
        {
            var tags = tagging.GetTagList();

            try
            {
                switch (input.Verb)
                {
                    case "help":
                        EchoHelp();
                        break;
                    case "test":
                        db.TestConnection();
                        ConsoleHelper.WriteSuccess("Connection OK");

                        if (!Directory.Exists(options.BackupFolder))
                            throw new ArgumentException("Backup foler doesn't exist. Edit app.config and create folder.");
                        ConsoleHelper.WriteSuccess("Backup folder OK");
                        db.TestDatabases();
                        break;
                    case "save":
                        if (input.ParamCount == 0)
                        {
                            EchoHelp();
                            return;
                        }

                        var tagName = input.P1;
                        if (input.P1 == "now")
                            tagName = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");

                        db.Backup(tagName);
                        break;
                    case "load":
                        if (input.ParamCount == 0)
                        {
                            EchoHelp();
                            return;
                        }

                        var loadParam = input.P1;
                        if (loadParam == "last")
                            loadParam = "0)";

                        if (loadParam.EndsWith(")"))
                        {
                            var position = ExtractPosition(loadParam);
                            var tag = tagging.GetTagAtPosition(position);
                            db.Restore(tag);
                        }
                        else
                        {
                            if (tags.Contains(loadParam))
                                db.Restore(loadParam);
                            else
                            {
                                var filteredTags = tagging.GetTagList(loadParam);
                                if (filteredTags.Count > 0)
                                {
                                    Console.WriteLine("Select backup number you want to restore, N to cancel:");
                                    for (var i = 0; i < filteredTags.Count; i++)
                                        Console.WriteLine(" {0}) {1}", i, filteredTags[i]);

                                    var userInput = Console.ReadLine();
                                    if (userInput != null && userInput.ToLower() != "n")
                                    {
                                        var position = ExtractPosition(userInput);

                                        if (position > -1)
                                        {
                                            var tag = tagging.GetTagAtPosition(position, loadParam);
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
                        if (input.ParamCount == 0)
                        {
                            EchoHelp();
                            return;
                        }

                        if (input.P1.EndsWith(")"))
                        {
                            tagging.DeleteAtPosition(ExtractPosition(input.P1));
                        }
                        else
                        {
                            tagging.Delete(input.P1);
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
                    case "zip":
                        if (input.ParamCount == 0)
                        {
                            EchoHelp();
                            return;
                        }

                        var zipParam = input.P1;
                        if (zipParam == "last")
                            zipParam = "0)";

                        if (zipParam.EndsWith(")"))
                        {
                            var position = ExtractPosition(zipParam);
                            compress.Zip(position);
                        }
                        else
                        {
                            if (tags.Contains(zipParam))
                                compress.Zip(zipParam);
                            else
                            {
                                var filteredTags = tagging.GetTagList(zipParam);
                                if (filteredTags.Count > 0)
                                {
                                    Console.WriteLine("Select tag number you want to zip, N to cancel:");
                                    for (var i = 0; i < filteredTags.Count; i++)
                                        Console.WriteLine(" {0}) {1}", i, filteredTags[i]);

                                    var userInput = Console.ReadLine();
                                    if (userInput != null && userInput.ToLower() != "n")
                                    {
                                        var position = ExtractPosition(userInput);

                                        if (position > -1)
                                        {
                                            var tag = tagging.GetTagAtPosition(position, zipParam);
                                            compress.Zip(tag);
                                        }
                                    }
                                }
                                else throw new ArgumentException("Cannot zip non-existing tag");
                            }
                        }
                        break;
                    case "unzip":
                        if (input.ParamCount == 0)
                        {
                            EchoHelp();
                            return;
                        }

                        var unzipParam = input.P1;
                        var zips = compress.GetZipList();

                        if (zips.Contains(unzipParam))
                            compress.Unzip(unzipParam);
                        else
                        {
                            var filteredZips = compress.GetZipList(unzipParam);
                            if (filteredZips.Count > 0)
                            {
                                Console.WriteLine("Select tag you want to unzip, N to cancel:");
                                for (var i = 0; i < filteredZips.Count; i++)
                                    Console.WriteLine(" {0}) {1}", i, filteredZips[i]);

                                var userInput = Console.ReadLine();
                                if (userInput != null && userInput.ToLower() != "n")
                                {
                                    var position = ExtractPosition(userInput);

                                    if (position > -1)
                                    {
                                        var zip = compress.GetZipAtPosition(position, unzipParam);
                                        compress.Unzip(zip);
                                    }
                                }
                            }
                            else throw new ArgumentException("Cannot load non-existing tag");
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError(ex.Message);
            }
        }

        static int ExtractPosition(string input)
        {
            int position = -1;
            if (input.Length > 1 && input.EndsWith(")"))
            {
                position = int.Parse(input.Remove(input.Length - 1));
            }
            else
            {
                position = int.Parse(input);
            }

            return position;
        }

        static void EchoHelp()
        {
            Console.WriteLine("USAGE: ");
            Console.WriteLine("    list                         list all created tags");
            Console.WriteLine("    save <tag> | now             create database backup to specified folder");
            Console.WriteLine("    load <tag> | last | <num>)   restore backup from specific tag or number");
            Console.WriteLine("    delete <tag> | <num>)        delete database backup by tag name or number");
            Console.WriteLine("    zip <tag> | last | <num>)    zip database backup by tag name or number");
            Console.WriteLine("    unzip <tag>                  unzip database backup by tag name");
            Console.WriteLine("    drop                         deletes database from server");
            Console.WriteLine("    test                         test settings are correct");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES: ");
            Console.WriteLine("    dbtool list                  list all tags, with their numbers");
            Console.WriteLine("    dbtool save now              create db backup with current timestamp as tag");
            Console.WriteLine("    dbtool save myTag            create db backup with name myTag");
            Console.WriteLine("    dbtool load myTag            restore db backup with name myTag");
            Console.WriteLine("    dbtool load last             restore last backup from backup folder");
            Console.WriteLine("    dbtool load my               offer all tags starting with input");
            Console.WriteLine("    dbtool load 2)               load 2nd restore (visible using list command)");
            Console.WriteLine("    dbtool delete myTag          delete backup tagged with myTag");
            Console.WriteLine("    dbtool delete 2)             delete 2nd backup (visible using list command)");
            Console.WriteLine("    dbtool drop                  drops databases from server");
        }
    }
}
