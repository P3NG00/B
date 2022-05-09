using B.Inputs;
using B.Utils;
using B.Utils.Enums;

namespace B.Modules.Tools.Indexer
{
    public sealed class ModuleIndexer : Module<ModuleIndexer.Stages>
    {
        #region Universal Properties

        public static string Title => "Indexer";
        public static string DirectoryPath => Program.DataPath + @"indexer\";
        public static string DirectoryDrivesPath => DirectoryPath + @"drives\";
        public static string SettingsPath => DirectoryPath + "settings";

        #endregion



        #region Private Variables

        private static IndexSettings Settings = null!;
        private static List<DriveIndexer> _driveIndexers = new();

        #endregion



        #region Private Properties

        private bool IsIndexing
        {
            get
            {
                foreach (var driveIndexer in _driveIndexers)
                {
                    if (driveIndexer.Indexing)
                        return true;
                }

                return false;
            }
        }

        #endregion



        #region Constructors

        public ModuleIndexer() : base(Stages.MainMenu) { }

        #endregion



        #region Override Methods

        public sealed override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Vector2 windowSize = new(29, 7);
                        Choice choice = new(Title);

                        if (!IsIndexing)
                        {
                            windowSize.y += _driveIndexers.Count + 2;
                            Window.Size = windowSize;

                            foreach (var driveIndexer in _driveIndexers)
                            {
                                string label = $"{driveIndexer.Drive.Name} ({driveIndexer.Drive.VolumeLabel})";
                                choice.AddKeybind(Keybind.CreateTogglable(driveIndexer.Index, label));
                            }

                            choice.AddSpacer();
                            // Keybind to begin indexing drives
                            choice.AddKeybind(Keybind.CreateConfirmation(() =>
                            {
                                // Start threads to scan each drive
                                _driveIndexers.ForEach(driveIndexer => driveIndexer.BeginIndex());
                                // Start thread to watch for end of indexing
                                ProgramThread.StartThread("IndexWatcher", () =>
                                {
                                    Util.WaitFor(() => !IsIndexing, 0.5f);
                                    // If on indexing screen, ensure text gets cleared
                                    if (Program.IsModuleOfType<ModuleIndexer>())
                                    {
                                        Window.Clear();
                                        Input.Action = Util.Void;
                                    }
                                });
                            }, "Begin indexing selected drives?", "Begin Indexing", '1'));
                            // Keybind to toggle indexing of network drives
                            choice.AddKeybind(Keybind.CreateTogglable(Settings.IndexNetworkDrives, "Network Drives", '9'));
                        }
                        else
                        {
                            Window.Size = windowSize;

                            // TODO display info about indexing
                            // TODO display all drives in list with percentage of complete it's done indexing

                            choice.AddText(new("Indexing...", PrintType.Highlight));
                        }

                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateModuleExit(this));
                        Cursor.y = 1;
                        choice.Request();
                    }
                    break;
            }
        }

        public sealed override void Save() => Data.Serialize(SettingsPath, Settings);

        public sealed override void Quit()
        {
            Save();
            base.Quit();
        }

        #endregion



        #region Universal Methods

        public static void Initialize()
        {
            // Directory check
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);
            if (!Directory.Exists(DirectoryDrivesPath))
                Directory.CreateDirectory(DirectoryDrivesPath);
            // Load settings
            Settings = File.Exists(SettingsPath) ? Data.Deserialize<IndexSettings>(SettingsPath) : new();
            // Set togglable function
            Settings.IndexNetworkDrives.SetOnChangeAction(() =>
            {
                UpdateDriveIndexers();
                Window.Clear();
            });
            // Update drives
            UpdateDriveIndexers();

            // TODO make indexer run on startup on drives that haven't been checked before. use default settings: non-network drives
        }

        #endregion



        #region Private Methods

        private static void UpdateDriveIndexers()
        {
            _driveIndexers.Clear();
            // Get all ready drives
            List<DriveInfo> drivesList = new(DriveInfo.GetDrives());
            // Remove drives that can't be read
            drivesList.RemoveAll(drive => !drive.IsReady);
            // Check to remove network drives
            if (!Settings.IndexNetworkDrives)
                drivesList.RemoveAll(drive => drive.DriveType == DriveType.Network);
            // Add each remaining drive to indexable list
            drivesList.ForEach(drive => _driveIndexers.Add(new(drive)));
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
