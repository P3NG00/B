using System.IO.Compression;
using Newtonsoft.Json;

namespace B.Modules.Tools.Indexer
{
    public sealed class IndexInfo
    {
        #region Private Variables

        [JsonProperty] private readonly List<string> _files = new();
        [JsonProperty] private readonly List<string> _hiddenData = new();
        [JsonProperty] private readonly List<string> _compressedData = new();
        [JsonProperty] private readonly List<string> _unauthorizedData = new();

        #endregion



        #region Public Methods

        public void AddFile(FileInfo file) => _files.Add(GetName(file));

        public void AddHiddenData(FileSystemInfo info) => _hiddenData.Add(GetName(info));

        public void AddCompressedData(FileInfo zipFile, ZipArchiveEntry entry) => _compressedData.Add(GetName(zipFile, entry));

        public void AddUnauthorizedData(FileSystemInfo info) => _unauthorizedData.Add(GetName(info));

        #endregion



        #region Private Methods

        private string GetName(FileSystemInfo info)
        {
            string s = ReplaceSlashes(info.FullName);

            if (info is DirectoryInfo)
                s += '/';

            return s;
        }

        private string GetName(FileInfo zipFile, ZipArchiveEntry entry) => ReplaceSlashes($"{zipFile.FullName}/{entry.FullName}");

        private string ReplaceSlashes(string s) => s.Replace('\\', '/');

        #endregion
    }
}
