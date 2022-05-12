using Newtonsoft.Json;

namespace B.Modules.Tools.Indexer
{
    public sealed class IndexInfo
    {
        #region Private Variables

        // Drive info
        [JsonProperty] private readonly string _driveFormat;
        [JsonProperty] private readonly string _driveType;
        [JsonProperty] private readonly string _driveName;
        [JsonProperty] private readonly long _driveFreeSpace;
        [JsonProperty] private readonly long _driveTotalSize;
        [JsonProperty] private readonly string _driveLabel;

        // Lists
        [JsonProperty] private readonly List<string> _files = new();
        [JsonProperty] private readonly List<string> _inaccessibleData = new();

        // Index info
        [JsonProperty] private bool _finished = false;

        #endregion



        #region Public Properties

        [JsonIgnore] public bool Finished => _finished;

        #endregion



        #region Constructors

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

        public void Add(FileSystemInfo info) => _files.Add(GetName(info));

        public void AddInaccessibleData(FileSystemInfo info) => _inaccessibleData.Add(GetName(info));

        public void Finish()
        {
            if (ModuleIndexer.ProcessIndexing)
                _finished = true;
        }

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
