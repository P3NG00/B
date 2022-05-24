using B.Inputs;
using B.Modules;
using B.Modules.Games.Adventure;
using B.Modules.Games.Blackjack;
using B.Modules.Games.MadLibs;
using B.Modules.Games.MexicanTrain;
using B.Modules.Games.NumberGuesser;
using B.Modules.Games.Checkers;
using B.Modules.Tools.FTP;
using B.Modules.Tools.Indexer;
using B.Modules.Tools.MoneyTracker;
using B.Modules.Tools.Settings;
using B.Modules.Toys.BrainFuck;
using B.Modules.Toys.Canvas;
using B.Modules.Toys.TextGenerator;
using B.Utils;
using B.Utils.Themes;

namespace B
{
    public sealed class Program : Module<Program.Stages>
    {
        #region Constants

        // Program Title.
        public const string Title = "B";

        #endregion



        #region Code Entry Point

        // Where code begins executing.
        public static void Main() => new Program().Start();

        #endregion



        #region Universal Properties

        // Relative path to program's data folder.
        public static string DataPath => Environment.CurrentDirectory + @"\data\";
        // Current program settings.
        public static ProgramSettings Settings { get; private set; } = null!;
        // Program instance reference.
        public static Program Instance { get; private set; } = null!;

        #endregion



        #region Private Variables

        // Currently selected Module Group.
        private ModuleGroup _selectedModuleGroup = null!;
        // Currently selected Module.
        private IModule? _selectedModule = null!;

        #endregion



        #region Private Properties

        // Organized list of Module Groups.
        private static ModuleGroup[] ModuleGroups => new ModuleGroup[]
        {
            new("Games",
                typeof(ModuleAdventure),
                typeof(ModuleMexicanTrain),
                typeof(ModuleBlackjack),
                typeof(ModuleCheckers),
                typeof(ModuleMadLibs),
                typeof(ModuleNumberGuesser)
            ),
            new("Toys",
                typeof(ModuleCanvas),
                typeof(ModuleBrainFuck),
                typeof(ModuleTextGenerator)
            ),
            new("Tools",
                typeof(ModuleFTP),
                typeof(ModuleMoneyTracker),
                typeof(ModuleIndexer),
                typeof(ModuleSettings)
            )
        };

        #endregion



        #region Constructor

        // Sets the Singleton reference. Only meant to be constructed once.
        // Attempting to construct another Program will throw an exception.
        public Program() : base(Stages.Program)
        {
            // Singleton check
            if (Instance is not null)
                throw new Exception("Program already instantiated.");
            // Set instance
            Instance = this;
        }

        #endregion



        #region Private Methods

        // Starts the program.
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
                catch (NotImplementedException) { HandleException(headerText: new Text("This feature is not yet implemented.", new ColorPair(ConsoleColor.DarkYellow, ConsoleColor.Gray))); }
                catch (Exception e) { HandleException(e); }
            }

            // Attempt to remove existing lock to let other threads finish properly
            ProgramThread.TryUnlock();

            // Mark indexers to finish during Goodbye screen
            ModuleIndexer.ProcessIndexing = false;

            // Check to begin Goodbye wink
            bool displayGoodbye = Settings.DisplayGoodbye;

            if (displayGoodbye)
            {
                // Disable debug mode
                if (Settings.DebugMode)
                    Settings.DebugMode.Toggle();

                // Display Goodbye screen
                Window.Clear();
                Cursor.Set(7, 3);
                Window.Print("Goodbye!");
                Cursor.Set(10, 5);
                Window.Print(":)");
                ProgramThread.Wait(1);
            }

            // Save settings
            try { Data.Serialize(ProgramSettings.Path, Settings); }
            catch (Exception e) { HandleException(e); }

            // Wait for indexers to finish
            Util.WaitFor(() => !ModuleIndexer.IsIndexing, 0.25f);

            // Check Goodbye screen again
            if (displayGoodbye)
            {
                // Finish Goodbye screen wink
                Cursor.Set(10, 5);
                Window.Print(";");
                ProgramThread.Wait(0.5f);
                Cursor.Set(10, 5);
                Window.Print(":");
                ProgramThread.Wait(1);
            }
        }

        // Initializes the program before accepting user input.
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
                Settings = new();

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

        // Program loop.
        public sealed override void Loop()
        {
            // Lock thread
            ProgramThread.TryLock();

            switch (Stage)
            {
                case Stages.Program:
                    {
                        // Display main menu modules
                        Window.SetSize(22, ModuleGroups.Length + 6);
                        Cursor.Set(0, 1);
                        Choice choice = new(Title);
                        char c = '1';
                        foreach (var group in ModuleGroups)
                        {
                            choice.AddKeybind(Keybind.Create(() =>
                            {
                                _selectedModuleGroup = group;
                                SetStage(Stages.Group);
                            }, group.GroupTitle, c++));
                        }
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateModuleExit(this));
                        choice.Request();
                    }
                    break;

                case Stages.Group:
                    {
                        // Display selected group level modules
                        Window.SetSize(22, _selectedModuleGroup.ModuleTypes.Length + 6);
                        Cursor.Set(0, 1);
                        Choice choice = new(_selectedModuleGroup.GroupTitle);
                        char c = '1';
                        foreach (var moduleType in _selectedModuleGroup.ModuleTypes)
                        {
                            string title = (string)moduleType.GetProperty("Title")?.GetValue(null)!;

                            if (title is null)
                                throw new Exception($"{moduleType.Name} has no 'Title' property.");

                            choice.AddKeybind(Keybind.Create(() =>
                            {
                                _selectedModule = (IModule)Activator.CreateInstance(moduleType)!;
                                SetStage(Stages.Module);
                            }, title, c++));
                        }
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateModuleExit(this));
                        choice.Request();
                    }
                    break;

                case Stages.Module:
                    {
                        // Run module
                        if (IsSelectedModuleValid())
                            _selectedModule!.Loop();

                        // Check module
                        if (!IsSelectedModuleValid())
                        {
                            _selectedModule = null;
                            SetStage(Stages.Group);
                            ProgramThread.TryUnlock();
                        }

                        // Local function
                        bool IsSelectedModuleValid() => _selectedModule is not null && _selectedModule.IsRunning;
                    }
                    break;
            }

            // Clear Keybinds
            Keybind.ClearRegisteredKeybinds();
        }

        // Handles backing out from the program or the module group.
        public sealed override void Quit()
        {
            // If quit is pressed in Stages.Group go back to Stages.Program
            if (Stage == Stages.Group)
                SetStage(Stages.Program);
            else
                base.Quit();
        }

        #endregion



        #region Universal Methods

        // Handles exceptions and allows the program to continue running.
        public static void HandleException(Exception? exception = null, Text? headerText = null)
        {
            // Ensure thread is locked while processing
            ProgramThread.TryLock();
            Keybind.ClearRegisteredKeybinds();
            Window.Clear();
            Window.Size = Window.SizeMax * 0.75f;
            // Cursor will always start at (2, 1)
            Cursor.Set(2, 1);

            if (headerText is null)
                Window.Print("An exception was thrown!", new ColorPair(colorText: ConsoleColor.Red));
            else
                headerText.Print();

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
            // Clear newly registered keybinds
            Keybind.ClearRegisteredKeybinds();
            // Set to previous stage
            Program pInst = Instance;
            pInst._selectedModule = null;

            if (pInst.Stage == Stages.Module)
                pInst.SetStage(Stages.Group);
            else if (pInst.Stage == Stages.Group)
                pInst.SetStage(Stages.Program);
        }

        // Used to check what Module is currently selected.
        public static bool IsModuleOfType<T>() where T : IModule => Instance._selectedModule is T;

        #endregion



        #region Enum

        // Stages of the program.
        public enum Stages
        {
            // Base level to select Module Groups.
            Program,
            // Level to select a Module from a Module Group.
            Group,
            // Level where Modules are run.
            Module,
        }

        #endregion
    }
}
