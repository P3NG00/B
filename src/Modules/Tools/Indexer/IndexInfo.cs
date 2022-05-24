using Newtonsoft.Json;

namespace B.Modules.Tools.Indexer
{
    public sealed class IndexInfo
    {
        #region Private Variables

        // Format type of drive.
        [JsonProperty] private readonly string _driveFormat;
        // Type of drive.
        [JsonProperty] private readonly string _driveType;
        // Drive letter.
        [JsonProperty] private readonly string _driveName;
        // Free space on drive.
        [JsonProperty] private readonly long _driveFreeSpace;
        // Total space on drive.
        [JsonProperty] private readonly long _driveTotalSize;
        // Name of drive.
        [JsonProperty] private readonly string _driveLabel;

        // Indexed data.
        [JsonProperty] private readonly List<string> _data = new();
        // Inaccessible data.
        [JsonProperty] private readonly List<string> _inaccessibleData = new();

        // If the Indexer has finished indexing.
        [JsonProperty] private bool _finished = false;

        #endregion



        #region Public Properties

        // If the indexer has finished indexing.
        [JsonIgnore] public bool Finished => _finished;

        #endregion



        #region Constructors

        // Creates a new instance of IndexInfo.
        public IndexInfo(DriveInfo drive)
        {
            _driveFormat = drive.DriveFormat;
            _driveType = drive.DriveType.ToString();
            _driveName = drive.Name;
            _driveFreeSpace = drive.TotalFreeSpace;
            _driveTotalSize = drive.TotalSize;
            _driveLabel = drive.VolumeLabel;
        }

        #endregion



        #region Public Methods

        // Adds indexed data.
        public void Add(FileSystemInfo info) => _data.Add(GetName(info));

        // Adds inaccessible data.
        public void AddInaccessibleData(FileSystemInfo info) => _inaccessibleData.Add(GetName(info));

        // Marks the indexer as finished.
        public void Finish()
        {
            // This check is here since indexers can be stopped
            // before they are done indexing. If the indexer is
            // stopped before finishing naturally, it will be
            // marked as 'unfinished.'
            if (ModuleIndexer.ProcessIndexing)
                _finished = true;
        }

        #endregion



        #region Private Methods

        // Gets name of directory or file.
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
