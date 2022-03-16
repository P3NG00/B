using B.Inputs;
using B.Options;
using B.Options.Games.Adventure;
using B.Options.Games.Blackjack;
using B.Options.Games.MexicanTrain;
using B.Options.Games.NumberGuesser;
using B.Options.Games.OptionCheckers;
using B.Options.Tools.Backup;
using B.Options.Tools.FTP;
using B.Options.Tools.MoneyTracker;
using B.Options.Tools.Settings;
using B.Options.Toys.BrainFuck;
using B.Options.Toys.Canvas;
using B.Utils;
using B.Utils.Extensions;

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
        public static ProgramSettings Settings { get; private set; } = new();
        public static string DataPath => Environment.CurrentDirectory + @"\data\";
        public static Levels CurrentLevel => _instance.Stage;

        // Option Groups
        private static (string GroupTitle, (Type OptionType, Func<string> GetTitle)[] Options)[] OptionGroups => new (string, (Type, Func<string>)[])[]
        {
            ("Games", new (Type, Func<string>)[] {
                (typeof(OptionAdventure), () => OptionAdventure.Title),
                (typeof(OptionMexicanTrain), () => OptionMexicanTrain.Title),
                (typeof(OptionBlackjack), () => OptionBlackjack.Title),
                (typeof(OptionCheckers), () => OptionCheckers.Title),
                (typeof(OptionNumberGuesser), () => OptionNumberGuesser.Title),
            }),
            ("Toys", new (Type, Func<string>)[] {
                (typeof(OptionCanvas), () => OptionCanvas.Title),
                (typeof(OptionBrainFuck), () => OptionBrainFuck.Title),
            }),
            ("Tools", new (Type, Func<string>)[] {
                (typeof(OptionFTP), () => OptionFTP.Title),
                (typeof(OptionBackup), () => OptionBackup.Title),
                (typeof(OptionMoneyTracker), () => OptionMoneyTracker.Title),
                (typeof(OptionSettings), () => OptionSettings.Title),
            }),
        };
        private static Program _instance = null!;

        private (string GroupTitle, (Type OptionType, Func<string> GetTitle)[] Options) _optionGroup;
        private IOption? _selectedOption = null;

        public Program() : base(Levels.Program)
        {
            if (_instance is null)
                _instance = this;
            else
                throw new Exception("Program already instantiated.");
        }

        private void Start()
        {
            // Initialize program
            try { Initialize(); }
            catch (Exception e)
            {
                HandleInitializationException(e);
                Environment.Exit(1);
            }

            // Program loop
            while (IsRunning)
            {
                try { Loop(); }
                catch (NotImplementedException) { HandleNotImplementedException(); }
                catch (Exception e) { HandleException(e); }
            }

            // Save before exiting
            try { Data.Serialize(ProgramSettings.Path, Program.Settings); }
            catch (Exception e) { HandleException(e); }
        }

        private void Initialize()
        {
            // Initialize window properties
            External.Initialize();

            // Set console settings
            Console.Title = Program.Title;

            // Console input ctrl+c override
            if (OperatingSystem.IsWindows())
                Console.TreatControlCAsInput = true;

            // Load program settings
            if (File.Exists(ProgramSettings.Path))
            {
                // If settings can't be loaded, just handle exception.
                // Settings are already initialized to default values.
                try { Program.Settings = Data.Deserialize<ProgramSettings>(ProgramSettings.Path); }
                catch (Exception) { }
            }

            // Update program settings
            Program.Settings.UpdateAll();
        }

        public override void Loop()
        {
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
                        Cursor.Position = new(0, 1);
                        Input.Choice choice = Input.Choice.Create($"{Program.Title}'s");
                        choice.AddRoutine(keybinds =>
                        {
                            ((Action<int>)(i =>
                            {
                                var optionGroup = Program.OptionGroups[i];

                                keybinds.Add(new(() =>
                                {
                                    _optionGroup = optionGroup;
                                    SetStage(Levels.Group);
                                }, optionGroup.GroupTitle, (char)('1' + i)));
                            })).Loop(Program.OptionGroups.Length);
                        });
                        choice.AddSpacer();
                        choice.AddExit(this);
                        choice.Request();
                    }
                    break;

                case Levels.Group:
                    {
                        Window.Clear();
                        Window.SetSize(22, _optionGroup.Options.Length + 6);
                        Cursor.Position = new(0, 1);
                        Input.Choice choice = Input.Choice.Create(_optionGroup.GroupTitle);
                        choice.AddRoutine(keybinds =>
                        {
                            ((Action<int>)(i =>
                            {
                                var option = _optionGroup.Options[i];

                                keybinds.Add(new(() =>
                                {
                                    _selectedOption = (IOption?)Activator.CreateInstance(option.OptionType);
                                    SetStage(Levels.Option);
                                }, option.GetTitle(), (char)('1' + i)));
                            })).Loop(_optionGroup.Options.Length);
                        });
                        choice.AddSpacer();
                        choice.AddExit(this);
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

        private void HandleException(Exception e) => PrintExceptionOutput(() =>
        {
            Window.Print("An exception was thrown!", ConsoleColor.Red);
            Cursor.Position = new(2, 3);
            Window.Print(e, ConsoleColor.White, ConsoleColor.Black);
        });

        private void HandleNotImplementedException() => PrintExceptionOutput(() =>
        {
            Window.Print("This feature is not yet implemented.", ConsoleColor.DarkYellow);
        });

        private void HandleInitializationException(Exception e) => PrintExceptionOutput(() =>
        {
            Window.Print("AN ERROR HAS OCCURRED DURING INITIALIZATION.", ConsoleColor.DarkRed, ConsoleColor.Gray);
            Cursor.Position = new(2, 3);
            Window.Print(e, ConsoleColor.White, ConsoleColor.Black);
        });

        private void PrintExceptionOutput(Action printAction)
        {
            Window.Clear();
            Window.SetSize(Window.SIZE_MAX / 2);
            Cursor.Position = new(2, 1);
            // Cursor will always start at (2, 1)
            printAction();
            Input.WaitFor(ConsoleKey.F1);
            Window.Clear();
            _selectedOption = null;

            if (Stage == Levels.Option)
                SetStage(Levels.Group);
            else if (Stage == Levels.Group)
                SetStage(Levels.Program);
        }

        public override void Quit()
        {
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
