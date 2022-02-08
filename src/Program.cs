using System.Runtime.InteropServices;
using B.Inputs;
using B.Options;
using B.Options.Adventure;
using B.Options.BrainFuck;
using B.Options.FTP;
using B.Options.MoneyTracker;
using B.Options.NumberGuesser;
using B.Options.Settings;
using B.Utils;

namespace B
{
    public class Program : Option
    {
        // Code entry point
        // TODO allow for arguments to enter specific things using shortcuts
        public static int Main() => new Program().Start();

        public static string DataPath => Environment.CurrentDirectory + @"\data\";
        public static ProgramSettings Settings { get; private set; } = new();
        public static readonly Vector2 WINDOW_SIZE_MIN = new(15, 1);
        public static Vector2? WINDOW_SIZE_MAX => Program._window_size_max; // TODO test references when values is null
        private static Vector2? _window_size_max = null;

        // NEW OPTIONS ONLY NEED TO BE REGISTERED HERE
        private readonly Dict<string, Type> _optionDict = new(
            new("Adventure!", typeof(OptionAdventure)),
            new("BrainFuck", typeof(OptionBrainFuck)),
            new("FTP", typeof(OptionFTP)),
            new("Money Tracker", typeof(OptionMoneyTracker)),
            new("Number Guesser", typeof(OptionNumberGuesser)),
            new("Settings", typeof(OptionSettings)));

        private Option? _option = null;

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
            while (this.Running)
            {
                try { this.Loop(); }
                catch (Exception e) { this.HandleException(e); }
            }

            // Save before exiting
            try { this.Save(); }
            catch (Exception e)
            {
                this.HandleException(e);
                // Return 1 to indicate failure saving data
                return 1;
            }

            // If reached, return nothing
            return 0;
        }

        private void Initialize()
        {
            // Set console window title
            Console.Title = "B";

            // Console input ctrl+c override
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

            // Find max size of console window by increasing the size until it throws an error.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Vector2 size = new Vector2(Program.WINDOW_SIZE_MIN.x, Program.WINDOW_SIZE_MIN.y);

                try
                {
                    while (true)
                    {
                        Console.SetWindowSize(size.x + 1, Program.WINDOW_SIZE_MIN.y);
                        // If above statement doesn't throw an exception, increment maxWidth
                        size.x++;
                    }
                }
                catch (ArgumentOutOfRangeException) { }

                try
                {
                    while (true)
                    {
                        Console.SetWindowSize(Program.WINDOW_SIZE_MIN.x, size.y + 1);
                        // If above statement doesn't throw an exception, increment maxHeight
                        size.y++;
                    }
                }
                catch (ArgumentOutOfRangeException) { }

                Program._window_size_max = size;
            }

            // TODO add thread with sole purpose of animating the cursor position into different corners of the screen. only allow animating when not printing (may need static variable in Util like Util.IsPrinting that will need to be updated before and after code is done printing so the aniimation can run).
        }

        public override void Loop()
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

                Util.ClearConsole(24, consoleHeight);

                if (Program.Settings.DebugMode)
                {
                    Util.PrintLine();
                    Util.PrintLine("    DEBUG ON");
                }

                Input.Option iob = new("B's");
                int i;

                for (i = 0; i < this._optionDict.Length; i++)
                {
                    Pair<string, Type> optionEntry = this._optionDict[i];
                    iob.Add(() => this._option = (Activator.CreateInstance(optionEntry.ItemRight!) as Option)!, optionEntry.ItemLeft!, (char)('1' + i));
                }

                iob.AddSpacer()
                    .Add(() => this.Quit(), "Quit", key: ConsoleKey.Escape)
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
                            .Add(() => Directory.Delete(Program.DataPath, true), "Yes", key: ConsoleKey.Enter)
                            .Add(null!, "No", key: ConsoleKey.Escape)
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

        public override void Save() => Util.Serialize(ProgramSettings.Path, Program.Settings);

        private void UpdateColors()
        {
            Console.BackgroundColor = Program.Settings.DarkMode ? ConsoleColor.Black : ConsoleColor.White;
            Console.ForegroundColor = Program.Settings.DarkMode ? ConsoleColor.White : ConsoleColor.Black;
        }

        private void HandleException(Exception e)
        {
            // Go back to main menu if exception was caught
            this._option = null;
            Vector2? consoleSize = Program.WINDOW_SIZE_MAX;

            try
            {
                if (consoleSize is not null)
                    Util.ClearConsole(consoleSize.x, consoleSize.y);
                else
                    Util.ClearConsole(150, 50);
            }
            catch (Exception) { }

            Util.PrintLine();
            Util.PrintLine("  An exception was thrown!", colorText: ConsoleColor.Red);
            Util.PrintLine();
            Util.PrintLine($"  {e.ToString()}", colorText: ConsoleColor.White, colorBackground: ConsoleColor.Black);
            Util.WaitForKey(ConsoleKey.F1);
            Util.ClearConsole();
        }
    }
}
