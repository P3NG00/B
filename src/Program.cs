using System.Runtime.InteropServices;
using B.Inputs;
using B.Options;
using B.Options.Adventure;
using B.Options.FTP;
using B.Options.MoneyTracker;
using B.Options.NumberGuesser;
using B.Utils;

namespace B
{
    public class Program
    {
        // Code entry point
        // TODO allow for arguments to enter specific things using shortcuts
        public static void Main() => new Program().Start();

        public static string DataPath => Environment.CurrentDirectory + @"\data\";
        public static ProgramSettings Settings { get; private set; } = new();

        // NEW OPTIONS ONLY NEED TO BE REGISTERED HERE
        private readonly Dict<string, Type> _optionDict = new(
            new("Adventure!", typeof(OptionAdventure)),
            new("FTP", typeof(OptionFTP)),
            new("Money Tracker", typeof(OptionMoneyTracker)),
            new("Number Guesser", typeof(OptionNumberGuesser)));

        private Option? _option = null;
        private bool _running = true;

        private void Start()
        {
            this.Initialize();

            // Program loop
            while (this._running)
                this.Loop();

            // Save before exiting
            this.Save();
        }

        private void Initialize()
        {
            // Set console window title
            Console.Title = "B";

            // Console input ctrl+c
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.TreatControlCAsInput = true;

            // Load program settings
            if (File.Exists(ProgramSettings.Path))
            {
                // If settings can't be loaded, just handle excepion.
                // Settings are already initialized to default values.
                try { Program.Settings = Util.Deserialize<ProgramSettings>(ProgramSettings.Path); } catch (Exception) { }
            }

            // Set console colors
            this.UpdateColors();

            // TODO find max size of console window. if options are too big, resize them
        }

        private void Loop()
        {
            try
            {
                // If directory doesn't exist, create it and add hidden attribute
                if (!Directory.Exists(Program.DataPath))
                {
                    DirectoryInfo mainDirectory = Directory.CreateDirectory(Program.DataPath);
                    mainDirectory.Attributes = FileAttributes.Hidden;
                }

                // If option is running, execute option code
                if (this._option != null && this._option.Running)
                    this._option.Loop();
                else
                {
                    // Display main menu options
                    int consoleHeight = this._optionDict.Length + 6;

                    if (Program.Settings.DebugMode)
                        consoleHeight += 2;

                    Util.ClearConsole(20, consoleHeight);

                    if (Program.Settings.DebugMode)
                        Util.Print("DEBUG ON", 4, linesBefore: 1);

                    Input.Option iob = new("B's");

                    for (int i = 0; i < this._optionDict.Length; i++)
                    {
                        Pair<string, Type> optionEntry = this._optionDict[i];
                        iob.AddKeybind(new(() => this._option = (Activator.CreateInstance(optionEntry.Item2!) as Option)!, optionEntry.Item1!, (char)('1' + i)));
                    }

                    iob.AddSpacer()
                        .AddKeybind(new(() => this._running = false, "Quit", key: ConsoleKey.Escape))
                        .Request();
                }

                switch (Util.LastInput.Key)
                {
                    // Toggle black and white colors
                    case ConsoleKey.F10:
                        {
                            Util.ToggleBool(ref Program.Settings.DarkMode);
                            this.UpdateColors();
                        }
                        break;

                    // Key to delete saved data
                    case ConsoleKey.F11:
                        {
                            Util.ClearConsole(15, 6);
                            new Input.Option("Delete Data?")
                                .AddKeybind(new(() => Directory.Delete(Program.DataPath, true), "Yes", key: ConsoleKey.Enter))
                                .AddKeybind(new(null!, "No", key: ConsoleKey.Escape))
                                .Request();
                        }
                        break;

                    // Key to toggle debug mode
                    case ConsoleKey.F12:
                        {
                            Util.ToggleBool(ref Program.Settings.DebugMode);
                            // Toggling Debug mode clears console to avoid artifacts
                            Util.ClearConsole();
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                // Go back to main menu if exception was caught
                this._option = null;
                Util.SetConsoleSize(140, 30);
                Util.Print(e.ToString());
                Util.WaitForKey(ConsoleKey.F1);
                Util.ClearConsole();
            }
        }

        private void Save() => Util.Serialize(ProgramSettings.Path, Program.Settings);

        private void UpdateColors()
        {
            Console.BackgroundColor = Program.Settings.DarkMode ? ConsoleColor.Black : ConsoleColor.White;
            Console.ForegroundColor = Program.Settings.DarkMode ? ConsoleColor.White : ConsoleColor.Black;
        }
    }
}
