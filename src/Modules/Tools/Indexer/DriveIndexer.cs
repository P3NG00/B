using B.Utils;

namespace B.Modules.Tools.Indexer
{
    public sealed class DriveIndexer
    {
        #region Public Variables

        public readonly Togglable Index = new(true);

        #endregion



        #region Public Properties

        public DriveInfo Drive => _drive;
        public string DriveName => Drive.VolumeLabel;
        public string DisplayName => $"{Drive.Name} ({DriveName})";
        public char DriveLetter => Drive.Name[0];
        public string FileSaveName => $"{DriveLetter}_({DriveName})";
        public bool IsIndexing => _indexThread is not null && _indexThread.IsAlive;
        public int CompletedEstimate => (int)((_indexedSize * 100) / (Drive.TotalSize - Drive.TotalFreeSpace));

        #endregion



        #region Private Variables

        private readonly DriveInfo _drive;
        private Thread? _indexThread = null;
        private long _indexedSize = 0;

        #endregion



        #region Constructors

        public DriveIndexer(DriveInfo drive) => _drive = drive;

        #endregion



        #region Public Methods

        public void BeginIndex()
        {
            // Check if supposed to be indexed
            if (!Index)
                throw new Exception($"Drive '{DriveName}' is marked to not be indexed!");

            // Check if already indexing
            if (IsIndexing)
                throw new Exception($"Drive '{DriveName}' has already begun indexing!");

            // TODO test that when Mouse or Keyboard locks threads the thread below is not affected

            // Start indexing thread
            _indexThread = ProgramThread.StartThread($"Indexer-{DriveName}", () =>
            {
                IndexInfo indexInfo = new(_drive);
                try { IndexDirectory(_drive.RootDirectory, in indexInfo); }
                catch (Exception e) { Program.HandleException(e, new($"Exception caught while indexing drive '{DriveName}'!")); }
                indexInfo.Finish();
                string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string finished = indexInfo.Finished ? string.Empty : "_incomplete";
                string filePath = $"{ModuleIndexer.DirectoryDrivesPath}{dateTime}_{FileSaveName}{finished}.txt";
                Data.Serialize(filePath, indexInfo);
            }, ThreadPriority.Highest);
        }

        #endregion



        #region Private Methods

        private void IndexDirectory(DirectoryInfo directory, in IndexInfo index)
        {
            index.Add(directory);

            // Search subdirectories
            foreach (var subdir in directory.GetDirectories())
            {
                if (!ModuleIndexer.ProcessIndexing)
                    return;

                // Index subdirectories
                try { IndexDirectory(subdir, in index); }
                // If exception caught, mark directory as unauthorized
                catch (UnauthorizedAccessException) { index.AddInaccessibleData(subdir); }
            }

            // Index files in current directory
            foreach (var file in directory.GetFiles())
            {
                if (!ModuleIndexer.ProcessIndexing)
                    return;

                index.Add(file);
                _indexedSize += file.Length;
            }
        }

        #endregion
    }
}
