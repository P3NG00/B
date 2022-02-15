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
    public sealed class Program
    {
        // Code entry point
        public static int Main() => new Program().Start();

        // Program Info
        public static string DataPath => Environment.CurrentDirectory + @"\data\";
        public static ProgramSettings Settings { get; private set; } = new();
        public static readonly Vector2 WINDOW_SIZE_ERROR = new(150, 50);
        public static readonly Vector2 WINDOW_SIZE_MIN = new(15, 1);
        public static Vector2? WINDOW_SIZE_MAX => Program._windowSizeMax;
        private static Vector2? _windowSizeMax = null;

        // Private Variables
        private readonly Dict<string, Type> _options = new(
            new("Adventure!", typeof(OptionAdventure)),
            new("BrainFuck", typeof(OptionBrainFuck)),
            new("FTP", typeof(OptionFTP)),
            new("Money Tracker", typeof(OptionMoneyTracker)),
            new("Number Guesser", typeof(OptionNumberGuesser)));
        private IOption _option = null!;
        private bool _running = true;

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
            while (this._running)
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
            // Set console window title
            Console.Title = "B";

            // Console input ctrl+c override
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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

            // Find max size of console window by increasing the size until it throws an error.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Vector2 size = Program.WINDOW_SIZE_MIN.Copy();

                try
                {
                    while (true)
                    {
                        Console.SetWindowSize(size.x + 2, Program.WINDOW_SIZE_MIN.y);
                        // If above statement doesn't throw an exception, increment maxWidth
                        size.x += 2;
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

                Program._windowSizeMax = size;
            }

            // TODO add thread with sole purpose of animating the cursor position into different corners of the screen. only allow animating when not printing (may need static variable in Util like Util.IsPrinting that will need to be updated before and after code is done printing so the animation can run).
        }

        public void Loop()
        {
            // If directory doesn't exist, create it and add hidden attribute
            if (!Directory.Exists(Program.DataPath))
            {
                DirectoryInfo mainDirectory = Directory.CreateDirectory(Program.DataPath);
                mainDirectory.Attributes = FileAttributes.Hidden;
            }

            // If option is running, execute option code
            if (this._option != null && this._option.IsRunning())
                this._option.Loop();
            else
            {
                // Display main menu options
                Util.ClearConsole(24, this._options.Length + 7);
                Input.Option iob = new("B's");

                for (int i = 0; i < this._options.Length; i++)
                {
                    Pair<string, Type> optionEntry = this._options[i];
                    iob.Add(() => this._option = (IOption)Activator.CreateInstance(optionEntry.ItemRight)!, optionEntry.ItemLeft, (char)('1' + i));
                }

                iob.AddSpacer()
                    .Add(() => this._option = Activator.CreateInstance<OptionSettings>(), "Settings", '9')
                    .Add(() => this._running = false, "Quit", key: ConsoleKey.Escape)
                    .Request();
            }

            if (Util.LastInput.Key == ConsoleKey.F12)
            {
                Util.ToggleBool(ref Program.Settings.DebugMode);
                // Toggling Debug mode clears console to avoid leftover characters
                Util.ClearConsole();
            }
        }

        private void HandleException(Exception e)
        {
            try
            {
                Vector2? maxConsoleSize = Program.WINDOW_SIZE_MAX;
                Util.ClearConsole(maxConsoleSize is not null ? maxConsoleSize : Program.WINDOW_SIZE_ERROR);
            }
            catch (Exception) { Util.ClearConsole(Program.WINDOW_SIZE_ERROR); }

            Util.PrintLine();
            Util.PrintLine("  An exception was thrown!", colorText: ConsoleColor.Red);
            Util.PrintLine();
            Util.PrintLine($"  {e.ToString()}", colorText: ConsoleColor.White, colorBackground: ConsoleColor.Black);
            Util.WaitForKey(ConsoleKey.F1);
            Util.ClearConsole();
            this._option = null!;
        }
    }
}
