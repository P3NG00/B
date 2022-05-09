using B.Inputs;
using B.Modules;
using B.Modules.Games.Adventure;
using B.Modules.Games.Blackjack;
using B.Modules.Games.MadLibs;
using B.Modules.Games.MexicanTrain;
using B.Modules.Games.NumberGuesser;
using B.Modules.Games.Checkers;
using B.Modules.Tools.Backup;
using B.Modules.Tools.FTP;
using B.Modules.Tools.Indexer;
using B.Modules.Tools.MoneyTracker;
using B.Modules.Tools.Settings;
using B.Modules.Toys.BrainFuck;
using B.Modules.Toys.Canvas;
using B.Modules.Toys.TextGenerator;
using B.Utils;
using B.Utils.Extensions;
using B.Utils.Themes;

namespace B
{
    public sealed class Program : Module<Program.Stages>
    {
        #region Constants

        public const string Title = "B";

        #endregion



        #region Code Entry Point

        public static void Main() => new Program().Start();

        #endregion



        #region Universal Properties

        public static string DataPath => Environment.CurrentDirectory + @"\data\";
        public static ProgramSettings Settings { get; private set; } = null!;
        public static Program Instance { get; private set; } = null!;

        #endregion



        #region Private Variables

        // Module Groups
        private static (string GroupTitle, Type[] ModuleTypes)[] ModuleGroups => new (string, Type[])[]
        {
            ("Games", new Type[] {
                typeof(ModuleAdventure),
                typeof(ModuleMexicanTrain),
                typeof(ModuleBlackjack),
                typeof(ModuleCheckers),
                typeof(ModuleMadLibs),
                typeof(ModuleNumberGuesser),
            }),
            ("Toys", new Type[] {
                typeof(ModuleCanvas),
                typeof(ModuleBrainFuck),
                typeof(ModuleTextGenerator),
            }),
            ("Tools", new Type[] {
                typeof(ModuleFTP),
                typeof(ModuleBackup),
                typeof(ModuleMoneyTracker),
                typeof(ModuleIndexer),
                typeof(ModuleSettings),
            }),
        };

        private (string GroupTitle, Type[] ModuleTypes) _moduleGroup;
        private IModule? _selectedModule = null;

        #endregion



        #region Constructor

        public Program() : base(Stages.Program)
        {
            if (Instance is null)
                Instance = this;
            else
                throw new Exception("Program already instantiated.");
        }

        #endregion



        #region Private Methods

        private void Start()
        {
            // Initialize program
            try { Initialize(); }
            catch (Exception e)
            {
                HandleException(e, new Text("AN ERROR HAS OCCURRED DURING INITIALIZATION.", new ColorPair(ConsoleColor.DarkRed, ConsoleColor.Gray)));
                Environment.Exit(1);
            }

            // Program loop
            while (IsRunning)
            {
                try { Loop(); }
                catch (NotImplementedException) { HandleException(null, new Text("This feature is not yet implemented.", new ColorPair(ConsoleColor.DarkYellow, ConsoleColor.Gray))); }
                catch (Exception e) { HandleException(e); }
            }

            // Save before exiting
            try { Data.Serialize(ProgramSettings.Path, Settings); }
            catch (Exception e) { HandleException(e); }
        }

        private void Initialize()
        {
            // Print 'Loading' message while program initializes.
            Window.Print("Loading...");

            // Lock thread before others are initialized.
            ProgramThread.TryLock();

            // Set console settings
            Console.Title = Title;

            // Console input ctrl+c override
            if (OperatingSystem.IsWindows())
                Console.TreatControlCAsInput = true;

            // If directory doesn't exist, create it and add hidden attribute
            if (!Directory.Exists(DataPath))
            {
                DirectoryInfo mainDirectory = Directory.CreateDirectory(DataPath);
                mainDirectory.Attributes |= FileAttributes.Hidden;
            }

            // Attempt to Deserialize saved Settings before further initialization
            if (File.Exists(ProgramSettings.Path))
            {
                try { Settings = Data.Deserialize<ProgramSettings>(ProgramSettings.Path); }
                catch (Exception) { }
            }

            // If Settings not initialized, default
            if (Settings is null)
                Settings = new ProgramSettings();

            // Initialize Settings after Settings has been created
            Settings.Initialize();

            // Initialize other window properties
            External.Initialize();

            // Initialize Utilities
            Util.Initialize();

            // Initialize Input after Settings has been initialized due to Mouse class
            Input.Initialize();

            // Initialize Indexer
            ModuleIndexer.Initialize();

            // Clear window
            Window.Clear();
        }

        #endregion



        #region Override Methods

        public sealed override void Loop()
        {
            // Lock thread
            ProgramThread.TryLock();
            // Clear Keybinds
            Keybind.ClearRegisteredKeybinds();

            switch (Stage)
            {
                case Stages.Program:
                    {
                        // Display main menu modules
                        Window.SetSize(22, ModuleGroups.Length + 6);
                        Cursor.Set(0, 1);
                        Choice choice = new($"{Title}'s");
                        for (int i = 0; i < ModuleGroups.Length; i++)
                        {
                            var moduleGroup = ModuleGroups[i];
                            char c = (char)('1' + i);
                            choice.AddKeybind(Keybind.Create(() =>
                            {
                                _moduleGroup = moduleGroup;
                                SetStage(Stages.Group);
                            }, moduleGroup.GroupTitle, c));
                        }
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateModuleExit(this));
                        choice.Request();
                    }
                    break;

                case Stages.Group:
                    {
                        // Display selected group level modules
                        Window.SetSize(22, _moduleGroup.ModuleTypes.Length + 6);
                        Cursor.Set(0, 1);
                        Choice choice = new(_moduleGroup.GroupTitle);
                        _moduleGroup.ModuleTypes.ForEach((moduleType, i) =>
                        {
                            string title = (string)moduleType.GetProperty("Title")?.GetValue(null)!;

                            if (title is null)
                                throw new Exception($"{moduleType.Name} has no 'Title' property.");

                            choice.AddKeybind(Keybind.Create(() =>
                            {
                                _selectedModule = moduleType.Instantiate<IModule>();
                                SetStage(Stages.Module);
                            }, title, (char)('1' + i)));
                        });
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateModuleExit(this));
                        choice.Request();
                    }
                    break;

                case Stages.Module:
                    {
                        // Run module
                        if (_selectedModule is not null && _selectedModule.IsRunning)
                            _selectedModule.Loop();

                        // Check module
                        if (_selectedModule is null || !_selectedModule.IsRunning)
                        {
                            _selectedModule = null;
                            SetStage(Stages.Group);
                            ProgramThread.TryUnlock();
                        }
                    }
                    break;
            }
        }

        public sealed override void Quit()
        {
            // If quit is pressed in Levels.Group go back to Levels.Program
            if (Stage == Stages.Group)
                SetStage(Stages.Program);
            else
                base.Quit();
        }

        #endregion



        #region Universal Methods

        public static void HandleException(Exception? exception = null, Text? text = null)
        {
            // Ensure thread is locked while processing
            ProgramThread.TryLock();
            Keybind.ClearRegisteredKeybinds();
            Window.Clear();
            Window.Size = Window.SizeMax * 0.75f;
            // Cursor will always start at (2, 1)
            Cursor.Set(2, 1);

            if (text is null)
                Window.Print("An exception was thrown!", new ColorPair(colorText: ConsoleColor.Red));
            else
                text.Print();

            if (exception is not null)
            {
                Cursor.NextLine(2, 2);
                Window.Print(exception, new ColorPair(ConsoleColor.White, ConsoleColor.Black));
            }

            Cursor.NextLine(2, 2);
            Choice choice = new();
            choice.AddKeybind(Keybind.Create(description: "Press any key to continue..."));
            // Thread will unlock while waiting for input
            choice.Request();
            // Thread will lock while processing
            ProgramThread.TryLock();
            Program pInst = Instance;
            pInst._selectedModule = null;

            if (pInst.Stage == Stages.Module)
                pInst.SetStage(Stages.Group);
            else if (pInst.Stage == Stages.Group)
                pInst.SetStage(Stages.Program);
        }

        public static bool IsModuleOfType<T>() where T : IModule => Instance._selectedModule is T;

        #endregion



        #region Enum

        public enum Stages
        {
            Program,
            Group,
            Module,
        }

        #endregion
    }
}
