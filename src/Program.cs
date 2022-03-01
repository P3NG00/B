using B.Inputs;
using B.Options;
using B.Options.Adventure;
using B.Options.Backup;
using B.Options.BrainFuck;
using B.Options.Canvas;
using B.Options.ExpressionSolver;
using B.Options.FTP;
using B.Options.MoneyTracker;
using B.Options.NumberGuesser;
using B.Options.Settings;
using B.Utils;

namespace B
{
    public sealed class Program : Option<Program.Stages>
    {
        public const string Title = "B";

        // Code entry point
        public static int Main()
        {
            Program program = new Program();
            return program.Start();
        }

        // Program Info
        public static ProgramSettings Settings { get; private set; } = new();
        public static string DataPath => Environment.CurrentDirectory + @"\data\";

        // Private Variables
        private static (Type OptionType, Func<string> GetTitle)[] OptionList => new (Type, Func<string>)[] {
            new(typeof(OptionAdventure),        () => OptionAdventure.Title),
            new(typeof(OptionBrainFuck),        () => OptionBrainFuck.Title),
            new(typeof(OptionCanvas),           () => OptionCanvas.Title),
            new(typeof(OptionExpressionSolver), () => OptionExpressionSolver.Title),
            new(typeof(OptionFTP),              () => OptionFTP.Title),
            new(typeof(OptionMoneyTracker),     () => OptionMoneyTracker.Title),
            new(typeof(OptionNumberGuesser),    () => OptionNumberGuesser.Title),
            new(typeof(OptionBackup),           () => OptionBackup.Title),
            new(typeof(OptionSettings),         () => OptionSettings.Title)};
        private IOption _selectedOption = null!;

        public Program() : base(Stages.MainMenu) { }

        private int Start()
        {
            // Initialize program
            try { this.Initialize(); }
            catch (Exception e)
            {
                this.HandleException(e);
                Environment.Exit(1);
            }

            // Program loop
            while (this.IsRunning())
            {
                try { this.Loop(); }
                catch (Exception e) { this.HandleException(e); }
            }

            // Save before exiting
            try { Util.Serialize(ProgramSettings.Path, Program.Settings); }
            catch (Exception e) { this.HandleException(e); }

            // If reached, return nothing
            return 0;
        }

        private void Initialize()
        {
            // Initialize window properties
            External.Initialize();

            // Set console window title
            Console.Title = Program.Title;

            // Console input ctrl+c override
            if (OperatingSystem.IsWindows())
                Console.TreatControlCAsInput = true;

            // Load program settings
            if (File.Exists(ProgramSettings.Path))
            {
                // If settings can't be loaded, just handle exception.
                // Settings are already initialized to default values.
                try { Program.Settings = Util.Deserialize<ProgramSettings>(ProgramSettings.Path); }
                catch (Exception) { }
            }

            // Set console colors
            Program.Settings.UpdateColors();
        }

        public override void Loop()
        {
            // If directory doesn't exist, create it and add hidden attribute
            if (!Directory.Exists(Program.DataPath))
            {
                DirectoryInfo mainDirectory = Directory.CreateDirectory(Program.DataPath);
                mainDirectory.Attributes = FileAttributes.Hidden;
            }

            switch (this.Stage)
            {
                case Stages.MainMenu:
                    {
                        // Display main menu options
                        Window.ClearAndSetSize(22, Program.OptionList.Length + 6);
                        Input.Choice iob = new($"{Program.Title}'s");

                        for (int i = 0; i < Program.OptionList.Length; i++)
                        {
                            var optionEntry = Program.OptionList[i];
                            iob.Add(() =>
                            {
                                this._selectedOption = (IOption)Activator.CreateInstance(optionEntry.OptionType)!;
                                this.SetStage(Stages.Option);
                            }, optionEntry.GetTitle(), (char)('1' + i));
                        }

                        iob.AddExit(this)
                            .Request();
                    }
                    break;

                case Stages.Option:
                    {
                        if (this._selectedOption != null && this._selectedOption.IsRunning())
                            this._selectedOption.Loop();
                        else
                        {
                            this._selectedOption = null!;
                            this.SetStage(Stages.MainMenu);
                        }
                    }
                    break;
            }
        }

        private void HandleException(Exception e)
        {
            Window.ClearAndSetSize(Window.SIZE_MAX);
            Window.Print("An exception was thrown!", (2, 1), ConsoleColor.Red);
            Window.Print(e, (2, 3), ConsoleColor.White, ConsoleColor.Black);
            Vector2 cursorPos = Cursor.GetPositionVector();
            cursorPos.x = 2;
            cursorPos.y += 2;
            Input.WaitFor(ConsoleKey.F1, cursorPos);
            Window.Clear();
            this._selectedOption = null!;
            this.SetStage(Stages.MainMenu);
        }

        public enum Stages
        {
            MainMenu,
            Option,
        }
    }
}
