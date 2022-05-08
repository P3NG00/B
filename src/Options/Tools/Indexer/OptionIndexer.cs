using System.Data;
using B.Inputs;
using B.Utils;
using B.Utils.Enums;

namespace B.Options.Tools.Indexer
{
    public sealed class OptionIndexer : Option<OptionIndexer.Stages>
    {
        #region Universal Properties

        public static string Title => "Indexer";
        public static string DirectoryPath => Program.DataPath + @"indexer\";

        #endregion



        #region Private Variables

        private static Thread[] _indexThreads = null!;

        #endregion



        #region Private Properties

        private bool HasStartedIndexing => _indexThreads is not null;
        private bool IsIndexing
        {
            get
            {
                if (!HasStartedIndexing)
                    return false;

                foreach (var thread in _indexThreads)
                {
                    if (thread.IsAlive)
                        return true;
                }

                return false;
            }
        }

        #endregion



        #region Constructors

        public OptionIndexer() : base(Stages.MainMenu)
        {
            // Directory check
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);
        }

        #endregion



        #region Override Methods

        public sealed override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.SetSize(24, 7);
                        Choice choice = new(Title);

                        if (!HasStartedIndexing)
                        {
                            choice.AddKeybind(Keybind.CreateConfirmation(() =>
                            {
                                // Get all ready drives
                                // TODO add togglable to index network drives (make it only changable when index watcher thread is not running)
                                var drives = DriveInfo.GetDrives().Where(drive => drive.IsReady && drive.DriveType != DriveType.Network).ToArray();
                                _indexThreads = new Thread[drives.Length];
                                // Start threads to scan each drive
                                for (int i = 0; i < drives.Length; i++)
                                {
                                    int j = i;
                                    var drive = drives[j];
                                    _indexThreads[j] = ProgramThread.StartThread($"Indexer{drive.VolumeLabel}", () => BeginIndex(drive), ThreadPriority.Highest);
                                }
                                // Start thread to watch for end of indexing
                                ProgramThread.StartLoopedThread("IndexWatcher", () =>
                                {
                                    if (!IsIndexing)
                                    {
                                        _indexThreads = null!;
                                        // Action needs to be set to refresh the current window
                                        // TODO make this only happen when in the appropriate window because this will just refresh whatever window you're on when it finishes.
                                        Input.Action = Util.Void;
                                    }

                                    ProgramThread.Wait(1f);
                                }, () => HasStartedIndexing);
                            }, "Begin indexing drives?", "Start Indexing...", '1'));
                        }
                        else
                            choice.AddText(new("Indexing...", PrintType.Highlight));

                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateOptionExit(this));
                        Cursor.y = 1;
                        choice.Request();
                    }
                    break;
            }
        }

        #endregion



        #region Private Methods

        private void BeginIndex(DriveInfo drive)
        {
            IndexInfo index = new();
            Index(drive.RootDirectory, in index);
            Data.Serialize(DirectoryPath + drive.VolumeLabel + ".txt", index);
        }

        private void Index(DirectoryInfo directory, in IndexInfo index)
        {
            // TODO test that empty folders are indexed too

            // Search subdirectories
            foreach (var subdir in directory.GetDirectories())
            {
                try
                {
                    // Index subdirectories
                    Index(subdir, in index);
                }
                catch (UnauthorizedAccessException)
                {
                    // Index directory marked as unauthorized
                    index.AddUnauthorizedDirectory(subdir);
                }
            }

            // Index files in current directory
            foreach (var file in directory.GetFiles())
                index.AddFile(file);
        }

        #endregion



        #region Enums

        public enum Stages
        {
            MainMenu,
        }

        #endregion
    }
}