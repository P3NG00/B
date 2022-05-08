using Newtonsoft.Json;

namespace B.Options.Tools.Indexer
{
    public sealed class IndexInfo
    {
        #region Private Variables

        [JsonProperty] private readonly List<string> _files = new();
        [JsonProperty] private readonly List<string> _unauthorizedDirectories = new();

        #endregion



        #region Public Methods

        public void AddFile(FileInfo file) => _files.Add(file.FullName);

        public void AddUnauthorizedDirectory(DirectoryInfo directory) => _unauthorizedDirectories.Add(directory.FullName);

        #endregion
    }
}
