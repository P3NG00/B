using Newtonsoft.Json;

namespace B.Modules.Tools.Indexer
{
    public sealed class IndexInfo
    {
        #region Private Variables

        [JsonProperty] private readonly List<string> _files = new();
        [JsonProperty] private readonly List<string> _hiddenItems = new();
        [JsonProperty] private readonly List<string> _unauthorizedDirectories = new();

        #endregion



        #region Public Methods

        public void AddFile(FileInfo file) => _files.Add(GetName(file));

        public void AddHiddenItem(FileSystemInfo info) => _hiddenItems.Add(GetName(info));

        public void AddUnauthorizedDirectory(DirectoryInfo directory) => _unauthorizedDirectories.Add(GetName(directory));

        #endregion



        #region Private Methods

        private string GetName(FileSystemInfo info)
        {
            string s = info.FullName.Replace('\\', '/');

            if (info is DirectoryInfo)
                s += '/';

            return s;
        }

        #endregion
    }
}
