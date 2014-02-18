using System;
using System.IO;
using System.IO.Compression;

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
    }
}
