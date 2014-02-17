using System.Collections.Generic;

namespace dbtool
{
    public class Options
    {
        public string BackupFolder { get; set; }
        public string ConnectionString { get; set; }
        public List<string> Databases { get; set; } 
    }
}
