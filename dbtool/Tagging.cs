using System.Collections.Generic;
using System.IO;

namespace dbtool
{
    public class Tagging
    {
        private readonly Options _options;

        public Tagging(Options options)
        {
            _options = options;
        }

        public List<string> GetTagList()
        {
            var files = Directory.GetFiles(_options.BackupFolder, "*.bak");
            var tags = new List<string>();

            foreach (var file in files)
            {
                var filename = new FileInfo(file).Name;
                var extractTag = filename.Split('.')[0];
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
    }
}
