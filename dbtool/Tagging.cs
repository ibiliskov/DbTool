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

        public string GetLocationForDatabase(string databaseName, string tagName)
        {
            return Path.Combine(_options.BackupFolder, string.Format("{0}.{1}.bak", tagName, databaseName));
        }
    }
}
