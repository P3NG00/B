using B.Inputs;
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
        public static ProgramSettings Settings { get; private set; } = new();
        public static Program Instance { get; private set; } = null!;

        // Option Groups
        private static (string GroupTitle, Type[] OptionType)[] OptionGroups => new (string, Type[])[]
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
                HandleException(new Text("AN ERROR HAS OCCURRED DURING INITIALIZATION.", ConsoleColor.DarkRed, ConsoleColor.Gray), e);
                Environment.Exit(1);
            }

            // Program loop
            while (IsRunning)
            {
                try { Loop(); }
                catch (NotImplementedException) { HandleException(new Text("This feature is not yet implemented.", ConsoleColor.DarkYellow)); }
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
                        Action<int> action = i =>
                        {
                            var optionGroup = Program.OptionGroups[i];

                            choice.Add(() =>
                            {
                                _optionGroup = optionGroup;
                                SetStage(Levels.Group);
                            }, optionGroup.GroupTitle, (char)('1' + i));
                        };
                        action.Loop(Program.OptionGroups.Length);
                        choice.AddSpacer();
                        choice.AddExit(this);
                        choice.Request();
                    }
                    break;

                case Levels.Group:
                    {
                        Window.Clear();
                        Window.SetSize(22, _optionGroup.OptionTypes.Length + 6);
                        Cursor.Position = new(0, 1);
                        Input.Choice choice = Input.Choice.Create(_optionGroup.GroupTitle);
                        Action<int> action = i =>
                        {
                            Type optionType = _optionGroup.OptionTypes[i];
                            string title = (string)optionType.GetProperty("Title")?.GetValue(null)!;

                            if (title is null)
                                throw new Exception($"{optionType.Name} has no 'Title' property.");

                            choice.Add(() =>
                            {
                                _selectedOption = (IOption?)Activator.CreateInstance(optionType);
                                SetStage(Levels.Option);
                            }, title, (char)('1' + i));
                        };
                        action.Loop(_optionGroup.OptionTypes.Length);
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

        public static void HandleException(Text message, Exception? exception = null)
        {
            Window.Clear();
            Window.Size = Window.SIZE_MAX * 0.75f;
            // Cursor will always start at (2, 1)
            Cursor.Position = new(2, 1);

            if (message is null)
                Window.Print("An exception was thrown!", ConsoleColor.Red);
            else
                message.Print();

            if (exception is not null)
            {
                Cursor.x = 2;
                Cursor.y += 2;
                Window.Print(exception, ConsoleColor.White, ConsoleColor.Black);
            }

            Input.WaitFor(ConsoleKey.F1);
            Window.Clear();
            Instance._selectedOption = null;

            if (Instance.Stage == Levels.Option)
                Instance.SetStage(Levels.Group);
            else if (Instance.Stage == Levels.Group)
                Instance.SetStage(Levels.Program);
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
