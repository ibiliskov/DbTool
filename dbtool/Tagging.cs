using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace dbtool
{
    public class Tagging
    {
        private readonly Options _options;

        public Tagging(Options options)
        {
            _options = options;
        }

        public IList<string> GetTagList()
        {
            var backupDirectory = new DirectoryInfo(_options.BackupFolder);
            var files = backupDirectory.GetFiles("*.bak").OrderByDescending(f => f.CreationTime).ToList();
            var tags = new List<string>();

            foreach (var file in files)
            {
                var extractTag = file.Name.Split('.')[0];
                if (!tags.Contains(extractTag))
                {
                    tags.Add(extractTag);
                }
            }

            return tags;
        }

        public string GetLocationForDatabase(string databaseName, string tagName)
        {
            return Path.Combine(_options.BackupFolder, string.Format("{0}.{1}.bak", tagName, databaseName));
        }

        public void Delete(string tagname)
        {
            var tags = GetTagList();

            if (!tags.Contains(tagname))
                throw new ArgumentException("Cannot delete non-existing tag.");

            var files = Directory.GetFiles(_options.BackupFolder, "*.bak");

            foreach (var file in files)
            {
                var filename = new FileInfo(file).Name;
                var extractTag = filename.Split('.')[0];

                if (extractTag == tagname)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
