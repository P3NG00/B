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
    public sealed class Program : Option<Program.Levels>
    {
        public const string Title = "B";

        // Code entry point
        public static void Main()
        {
            Program program = new Program();
            program.Start();
        }

        // Program Info
        public static string DataPath => Environment.CurrentDirectory + @"\data\";
        public static ProgramSettings Settings { get; private set; } = null!;
        public static Program Instance { get; private set; } = null!;

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
                typeof(OptionSettings),
            }),
        };

        private (string GroupTitle, Type[] OptionTypes) _optionGroup;
        private IOption? _selectedOption = null;

        public Program() : base(Levels.Program)
        {
            if (Instance is null)
                Instance = this;
            else
                throw new Exception("Program already instantiated.");
        }

        private void Start()
        {
            // Initialize program
            try { Initialize(); }
            catch (Exception e)
            {
                HandleException(new Text("AN ERROR HAS OCCURRED DURING INITIALIZATION.", new ColorPair(ConsoleColor.DarkRed, ConsoleColor.Gray)), e);
                Environment.Exit(1);
            }

            // Program loop
            while (IsRunning)
            {
                try { Loop(); }
                catch (NotImplementedException) { HandleException(new Text("This feature is not yet implemented.", new ColorPair(ConsoleColor.DarkYellow, ConsoleColor.Gray))); }
                catch (Exception e) { HandleException(e); }
            }

            // Save before exiting
            try { Data.Serialize(ProgramSettings.Path, Program.Settings); }
            catch (Exception e) { HandleException(e); }
        }

        private void Initialize()
        {
            // Set console settings
            Console.Title = Program.Title;

            // Console input ctrl+c override
            if (OperatingSystem.IsWindows())
                Console.TreatControlCAsInput = true;

            // Attempt to Deserialize saved Settings before further initialization
            if (File.Exists(ProgramSettings.Path))
            {
                try { Program.Settings = Data.Deserialize<ProgramSettings>(ProgramSettings.Path); }
                catch (Exception) { }
            }

            // If Settings not initialized, default
            if (Program.Settings is null)
                Program.Settings = new ProgramSettings();

            // Initialize Settings after Program.Settings has been created
            Program.Settings.Initialize();

            // Initialize other window properties
            External.Initialize();
            // Initialize Utilities
            Util.Initialize();
            // Initialize Mouse Capture Thread after Program.Settings has been been initialized
            Mouse.Initialize();
        }

        public override void Loop()
        {
            // Set window to drawing mode.
            Window.IsDrawing = true;

            // If directory doesn't exist, create it and add hidden attribute
            if (!Directory.Exists(Program.DataPath))
            {
                DirectoryInfo mainDirectory = Directory.CreateDirectory(Program.DataPath);
                mainDirectory.Attributes |= FileAttributes.Hidden;
            }

            switch (Stage)
            {
                case Levels.Program:
                    {
                        // Display main menu options
                        Window.Clear();
                        Window.SetSize(22, Program.OptionGroups.Length + 6);
                        Cursor.Set(0, 1);
                        Input.Choice choice = Input.Choice.Create($"{Program.Title}'s");
                        Util.Loop(Program.OptionGroups.Length, i =>
                        {
                            var optionGroup = Program.OptionGroups[i];
                            Action action = () =>
                            {
                                _optionGroup = optionGroup;
                                SetStage(Levels.Group);
                            };
                            char c = (char)('1' + i);
                            choice.AddKeybind(Keybind.Create(action, optionGroup.GroupTitle, c));
                        });
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateOptionExit(this));
                        choice.Request();
                    }
                    break;

                case Levels.Group:
                    {
                        Window.Clear();
                        Window.SetSize(22, _optionGroup.OptionTypes.Length + 6);
                        Cursor.Set(0, 1);
                        Input.Choice choice = Input.Choice.Create(_optionGroup.GroupTitle);
                        _optionGroup.OptionTypes.ForEach((optionType, i) =>
                        {
                            string title = (string)optionType.GetProperty("Title")?.GetValue(null)!;

                            if (title is null)
                                throw new Exception($"{optionType.Name} has no 'Title' property.");

                            Action createInstanceAction = () =>
                            {
                                _selectedOption = optionType.Instantiate<IOption>();
                                SetStage(Levels.Option);
                            };
                            char c = (char)('1' + i);
                            Keybind keybind = Keybind.Create(createInstanceAction, title, c);
                            choice.AddKeybind(keybind);
                        });
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateOptionExit(this));
                        choice.Request();
                    }
                    break;

                case Levels.Option:
                    {
                        if (_selectedOption is not null && _selectedOption.IsRunning)
                            _selectedOption.Loop();
                        else
                        {
                            _selectedOption = null;
                            SetStage(Levels.Group);
                        }
                    }
                    break;
            }
        }

        public static void HandleException(Text text, Exception? exception = null)
        {
            Window.Clear();
            Window.SetSize(Window.SizeMax * 0.75f);
            // Cursor will always start at (2, 1)
            Cursor.Set(2, 1);

            if (text is null)
                Window.Print("An exception was thrown!", new ColorPair(colorText: ConsoleColor.Red));
            else
                text.Print();

            if (exception is not null)
            {
                Cursor.x = 2;
                Cursor.y += 2;
                Window.Print(exception, new ColorPair(ConsoleColor.White, ConsoleColor.Black));
            }

            Input.WaitFor(ConsoleKey.F1);
            Window.Clear();
            Program P = Instance;
            P._selectedOption = null;

            if (P.Stage == Levels.Option)
                P.SetStage(Levels.Group);
            else if (P.Stage == Levels.Group)
                P.SetStage(Levels.Program);
        }

        public static void HandleException(Exception? e) => HandleException(null!, e);

        public override void Quit()
        {
            // If quit is pressed in Levels.Group go back to Levels.Program
            if (Stage == Levels.Group)
                SetStage(Levels.Program);
            else
                base.Quit();
        }

        public enum Levels
        {
            Program,
            Group,
            Option,
        }
    }
}
