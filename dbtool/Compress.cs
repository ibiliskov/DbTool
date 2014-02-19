using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace dbtool
{
    public class Compress
    {
        private readonly Tagging _tagging;
        private readonly Options _options;

        public Compress(Tagging tagging, Options options)
        {
            _tagging = tagging;
            _options = options;
        }

        public void Zip(int position)
        {
            var tag = _tagging.GetTagAtPosition(position);
            Zip(tag);
        }

        public void Zip(string tagName)
        {
            var files = _tagging.GetTagFiles(tagName);

            var tempDir = Directory.CreateDirectory(Path.Combine(_options.BackupFolder, "Temp"));
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                File.Copy(file, Path.Combine(tempDir.FullName, fileInfo.Name), true);
            }

            ZipFile.CreateFromDirectory(tempDir.FullName, Path.Combine(_options.BackupFolder, tagName) + ".zip", CompressionLevel.Optimal, false);
            tempDir.Delete(true);
        }

        public void Unzip(string tagName)
        {
            var zipFile = tagName.EndsWith(".zip") ? tagName : tagName + ".zip";

            if (!File.Exists(zipFile))
                throw new ArgumentException("Cannot find zip file specified");

            ZipFile.ExtractToDirectory(zipFile, _options.BackupFolder);
            File.Delete(zipFile);
        }

        public IList<string> GetZipList()
        {
            var backupDirectory = new DirectoryInfo(_options.BackupFolder);
            var files = backupDirectory.GetFiles("*.zip").OrderByDescending(f => f.CreationTime).ToList();
            var zips = new List<string>();

            foreach (var file in files)
            {
                var extractTag = file.Name.Split('.')[0];
                if (!zips.Contains(extractTag))
                {
                    zips.Add(extractTag);
                }
            }

            return zips;
        }

        public IList<string> GetZipList(string filter)
        {
            var zips = GetZipList();
            return zips.Where(tag => tag.StartsWith(filter)).ToList();
        }

        public string GetZipAtPosition(int position, string filter = null)
        {
            var zips = filter == null ? GetZipList() : GetZipList(filter);

            return zips.ElementAt(position);
        }
    }
}
