﻿using B.Inputs;
using B.Options;
using B.Options.Games.Adventure;
using B.Options.Games.Blackjack;
using B.Options.Games.MadLibs;
using B.Options.Games.MexicanTrain;
using B.Options.Games.NumberGuesser;
using B.Options.Games.OptionCheckers;
using B.Options.Tools.Backup;
using B.Options.Tools.FTP;
using B.Options.Tools.Indexer;
using B.Options.Tools.MoneyTracker;
using B.Options.Tools.Settings;
using B.Options.Toys.BrainFuck;
using B.Options.Toys.Canvas;
using B.Options.Toys.TextGenerator;
using B.Utils;
using B.Utils.Extensions;
using B.Utils.Themes;

namespace B
{
    public sealed class Program : Option<Program.Stages>
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

        // Option Groups
        private static (string GroupTitle, Type[] OptionTypes)[] OptionGroups => new (string, Type[])[]
        {
            ("Games", new Type[] {
                typeof(OptionAdventure),
                typeof(OptionMexicanTrain),
                typeof(OptionBlackjack),
                typeof(OptionCheckers),
                typeof(OptionMadLibs),
                typeof(OptionNumberGuesser),
            }),
            ("Toys", new Type[] {
                typeof(OptionCanvas),
                typeof(OptionBrainFuck),
                typeof(OptionTextGenerator),
            }),
            ("Tools", new Type[] {
                typeof(OptionFTP),
                typeof(OptionBackup),
                typeof(OptionMoneyTracker),
                typeof(OptionIndexer),
                typeof(OptionSettings),
            }),
        };

        private (string GroupTitle, Type[] OptionTypes) _optionGroup;
        private IOption? _selectedOption = null;

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

            // If directory doesn't exist, create it and add hidden attribute
            if (!Directory.Exists(DataPath))
            {
                DirectoryInfo mainDirectory = Directory.CreateDirectory(DataPath);
                mainDirectory.Attributes |= FileAttributes.Hidden;
            }

            switch (Stage)
            {
                case Stages.Program:
                    {
                        // Display main menu options
                        Window.SetSize(22, OptionGroups.Length + 6);
                        Cursor.Set(0, 1);
                        Choice choice = new($"{Title}'s");
                        for (int i = 0; i < OptionGroups.Length; i++)
                        {
                            var optionGroup = OptionGroups[i];
                            char c = (char)('1' + i);
                            choice.AddKeybind(Keybind.Create(() =>
                            {
                                _optionGroup = optionGroup;
                                SetStage(Stages.Group);
                            }, optionGroup.GroupTitle, c));
                        }
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateOptionExit(this));
                        choice.Request();
                    }
                    break;

                case Stages.Group:
                    {
                        // Display selected group level options
                        Window.SetSize(22, _optionGroup.OptionTypes.Length + 6);
                        Cursor.Set(0, 1);
                        Choice choice = new(_optionGroup.GroupTitle);
                        _optionGroup.OptionTypes.ForEach((optionType, i) =>
                        {
                            string title = (string)optionType.GetProperty("Title")?.GetValue(null)!;

                            if (title is null)
                                throw new Exception($"{optionType.Name} has no 'Title' property.");

                            choice.AddKeybind(Keybind.Create(() =>
                            {
                                _selectedOption = optionType.Instantiate<IOption>();
                                SetStage(Stages.Option);
                            }, title, (char)('1' + i)));
                        });
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateOptionExit(this));
                        choice.Request();
                    }
                    break;

                case Stages.Option:
                    {
                        // Run option
                        if (_selectedOption is not null && _selectedOption.IsRunning)
                            _selectedOption.Loop();

                        // Check option
                        if (_selectedOption is null || !_selectedOption.IsRunning)
                        {
                            _selectedOption = null;
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
            pInst._selectedOption = null;

            if (pInst.Stage == Stages.Option)
                pInst.SetStage(Stages.Group);
            else if (pInst.Stage == Stages.Group)
                pInst.SetStage(Stages.Program);
        }

        #endregion



        #region Enum

        public enum Stages
        {
            Program,
            Group,
            Option,
        }

        #endregion
    }
}
