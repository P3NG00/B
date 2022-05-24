using B.Utils;

namespace B.Modules.Tools.Indexer
{
    public sealed class DriveIndexer
    {
        #region Public Variables

        // Defines if this drive should be indexed.
        public readonly Togglable Index = new(true);

        #endregion



        #region Public Properties

        // The drive object.
        public DriveInfo Drive => _drive;
        // The name of the drive.
        public string DriveName => Drive.VolumeLabel;
        // The name of the drive for displaying.
        public string DisplayName => $"{Drive.Name} ({DriveName})";
        // The letter of the drive.
        public char DriveLetter => Drive.Name[0];
        // The name to save the drive as.
        public string FileSaveName => $"{DriveLetter}_({DriveName})";
        // If this indexer is currently indexing.
        public bool IsIndexing => _indexThread is not null && _indexThread.IsAlive;
        // Returns an inaccurate count of indexed data.
        // (Inacessible data cannot be counted)
        public int CompletedEstimate => (int)((_indexedSize * 100) / (Drive.TotalSize - Drive.TotalFreeSpace));

        #endregion



        #region Private Variables

        // The drive to index.
        private readonly DriveInfo _drive;
        // Indexing thread reference.
        private Thread? _indexThread = null;
        // Approximate size of indexed data.
        private long _indexedSize = 0;

        #endregion



        #region Constructors

        // Creates a new DriveIndexer for the given drive.
        public DriveIndexer(DriveInfo drive) => _drive = drive;

        #endregion



        #region Public Methods

        // Begins the drive indexing process.
        public void BeginIndex()
        {
            // Check if supposed to be indexed
            if (!Index)
                throw new Exception($"Drive '{DriveName}' is marked to not be indexed!");

            // Check if already indexing
            if (IsIndexing)
                throw new Exception($"Drive '{DriveName}' has already begun indexing!");

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

        // Recursively indexes the given directory.
        private void IndexDirectory(DirectoryInfo directory, in IndexInfo index)
        {
            // Index current directory
            index.Add(directory);

            // Index subdirectories in the current directory
            foreach (var subdir in directory.GetDirectories())
            {
                // Check to stop processing
                if (!ModuleIndexer.ProcessIndexing)
                    return;

                // Index subdirectory
                try { IndexDirectory(subdir, in index); }
                // If exception caught, mark directory as unauthorized
                catch (UnauthorizedAccessException) { index.AddInaccessibleData(subdir); }
            }

            // Index files in current directory
            foreach (var file in directory.GetFiles())
            {
                // Check to stop processing
                if (!ModuleIndexer.ProcessIndexing)
                    return;

                // Index file
                index.Add(file);

                // Add file size
                _indexedSize += file.Length;
            }
        }

        #endregion
    }
}
