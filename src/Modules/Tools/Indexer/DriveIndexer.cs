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
        public string DriveName => _drive.VolumeLabel;
        public bool Indexing => _indexThread is not null && _indexThread.IsAlive;

        #endregion



        #region Private Variables

        private readonly DriveInfo _drive;
        private Thread? _indexThread = null;

        #endregion



        #region Constructors

        public DriveIndexer(DriveInfo drive) => _drive = drive;

        #endregion



        #region Public Methods

        public void BeginIndex()
        {
            if (!Index)
                return;

            if (Indexing)
                throw new Exception($"Drive '{DriveName}' has already begun indexing!");

            // TODO test that when Mouse or Keyboard locks threads the thread below is not affected

            _indexThread = ProgramThread.StartThread($"Indexer-{DriveName}", () =>
            {
                IndexInfo indexInfo = new();
                IndexDirectory(_drive.RootDirectory, in indexInfo);
                Data.Serialize(ModuleIndexer.DirectoryDrivesPath + DriveName + ".txt", indexInfo);
            }, ThreadPriority.Highest);
        }

        #endregion



        #region Private Methods

        private void IndexDirectory(DirectoryInfo directory, in IndexInfo index)
        {
            // TODO index contents of compressed files (zip, rar, 7z, etc)

            // Index hidden directories
            if (IsHidden(directory))
                index.AddHiddenItem(directory);

            // Search subdirectories
            foreach (var subdir in directory.GetDirectories())
            {
                try
                {
                    // Index subdirectories
                    IndexDirectory(subdir, in index);
                }
                catch (UnauthorizedAccessException)
                {
                    // Index directory marked as unauthorized
                    index.AddUnauthorizedDirectory(subdir);
                }
            }

            // Index files in current directory
            foreach (var file in directory.GetFiles())
            {
                if (IsHidden(file))
                    index.AddHiddenItem(file);
                else
                    index.AddFile(file);
            }

            // Local functions
            bool IsHidden(FileSystemInfo info) => info.Attributes.HasFlag(FileAttributes.Hidden);
        }

        #endregion
    }
}
