using B.Inputs;
using B.Utils;
using B.Utils.Enums;

namespace B.Modules.Tools.Indexer
{
    public sealed class ModuleIndexer : Module<ModuleIndexer.Stages>
    {
        #region Universal Variables

        public static volatile bool ProcessIndexing = true;

        #endregion



        #region Universal Properties

        public static string Title => "Indexer";
        public static string DirectoryPath => Program.DataPath + @"indexer\";
        public static string DirectoryDrivesPath => DirectoryPath + @"drives\";
        public static string SettingsPath => DirectoryPath + "settings";
        public static bool IsIndexing
        {
            get
            {
                foreach (var driveIndexer in _driveIndexers)
                    if (driveIndexer.IsIndexing)
                        return true;

                return false;
            }
        }

        #endregion



        #region Private Variables

        private static IndexSettings Settings = null!;
        private static List<DriveIndexer> _driveIndexers = new();

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
                        int longestStringLength = 0;

                        foreach (var driveIndexer in _driveIndexers)
                        {
                            int driveDisplayNameLength = driveIndexer.DisplayName.Length;

                            if (driveDisplayNameLength > longestStringLength)
                                longestStringLength = driveDisplayNameLength;
                        }

                        Vector2 windowSize = new(30, 8);
                        Choice choice = new(Title);

                        if (!IsIndexing)
                        {
                            // Accommodate for longest drive name
                            FixWidth(19);
                            windowSize.y += _driveIndexers.Count + 3;
                            Window.Size = windowSize;
                            // Add each drive indexer
                            _driveIndexers.ForEach(driveIndexer => choice.AddKeybind(Keybind.CreateTogglable(driveIndexer.Index, driveIndexer.DisplayName)));
                            choice.AddSpacer();
                            // Togglable keybinds
                            choice.AddKeybind(Keybind.CreateTogglable(Settings.IndexNetworkDrives, "Network Drives", '8'));
                            choice.AddKeybind(Keybind.CreateTogglable(Settings.IndexOnStartup, "Index On Startup", '9'));
                            choice.AddSpacer();
                            // Keybind to begin indexing drives
                            choice.AddKeybind(Keybind.CreateConfirmation(BeginIndexing, "Begin indexing selected drives?", "Begin Indexing", '1'));
                        }
                        else
                        {
                            FixWidth(21);
                            int amountIndexers = _driveIndexers.Count;

                            if (amountIndexers > 0)
                                windowSize.y += amountIndexers + 2;

                            Window.Size = windowSize;
                            choice.AddText(new("Indexing...", PrintType.Highlight));
                            choice.AddSpacer();
                            // Display each drive indexer info
                            foreach (var driveIndexer in _driveIndexers)
                            {
                                bool indexing = driveIndexer.IsIndexing;
                                string percentage = "%" + $"{(indexing ? driveIndexer.CompletedEstimate : 100),3}";
                                string driveLabel = $"{percentage} {driveIndexer.DisplayName}";
                                PrintType printType = indexing ? PrintType.Highlight : PrintType.General;
                                choice.AddText(new(driveLabel, printType));
                            }
                            // Add stop indexing keybind
                            choice.AddSpacer();
                            // TODO keybind unclickable when window is reupdating. make keybinds stay registered until this window is left
                            choice.AddKeybind(Keybind.Create(() => ProcessIndexing = false, "Stop Indexing", key: ConsoleKey.End));
                        }

                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateModuleExit(this));
                        Cursor.y = 1;
                        choice.Request();

                        // Local functions
                        void FixWidth(int stringSizeOffset)
                        {
                            if (longestStringLength > stringSizeOffset)
                                windowSize.x += longestStringLength - stringSizeOffset;
                        }
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
            // Check startup indexing using last settings
            if (Settings.IndexOnStartup)
                BeginIndexing();
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

        private static void BeginIndexing()
        {
            if (IsIndexing)
                throw new Exception("Indexing is already in progress!");

            // Mark to index
            ProcessIndexing = true;
            // Remove all indexers that aren't marked to index
            _driveIndexers.RemoveAll(driveIndexer => !driveIndexer.Index);
            // Start remaining drive indexers
            _driveIndexers.ForEach(driveIndexer => driveIndexer.BeginIndex());
            // Start thread to watch for end of indexing
            ProgramThread.StartThread("IndexWatcher", () =>
            {
                // Wait for indexing to finish
                while (IsIndexing)
                {
                    if (IsInThisModule())
                        Window.Update();

                    ProgramThread.Wait(0.5f);
                }
                // Update drive indexers
                UpdateDriveIndexers();
                // If on indexing screen, ensure window gets reprinted
                if (IsInThisModule())
                {
                    Window.Update();
                    Window.Clear();
                }
                // Local functions
                bool IsInThisModule() => Program.IsModuleOfType<ModuleIndexer>();
            });
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
