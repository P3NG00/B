using B.Inputs;
using B.Options;
using B.Options.Adventure;
using B.Options.MoneyTracker;
using B.Options.NumberGuesser;
using B.Utils;

/* ================= *\
||                   ||
||        B's        ||
||                   ||
||  Created:         ||
||     2021.11.17    ||
||                   ||
||  Edited:          ||
||     2022.01.12    ||
||                   ||
\* ================= */

namespace B
{
    public class Program
    {
        public static readonly string DirectoryPath = Environment.CurrentDirectory + @"\data\";
        public static bool DebugMode { get { return Program._debugMode; } }
        private static bool _debugMode = false;
        private Option _option = null;
        private bool _running = true;

        private void Start()
        {
            Console.Title = "B";
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.TreatControlCAsInput = true;

            while (this._running)
            {
                try
                {
                    if (!Directory.Exists(Program.DirectoryPath))
                    {
                        DirectoryInfo mainDirectory = Directory.CreateDirectory(Program.DirectoryPath);
                        mainDirectory.Attributes = FileAttributes.Hidden;
                    }

                    if (this._option != null && this._option.IsRunning)
                        this._option.Loop();
                    else
                    {
                        Console.Clear();
                        int consoleHeight = 9;
                        if (Program.DebugMode) consoleHeight += 2;
                        Util.SetConsoleSize(20, consoleHeight);
                        if (Program.DebugMode) Util.Print("DEBUG ON", 4, linesBefore: 1);
                        new Input.Option("B's")
                            .AddKeybind(new Keybind(() => this._option = new OptionAdventure(), "Adventure", '1'))
                            .AddKeybind(new Keybind(() => this._option = new OptionNumberGuesser(), "Number Guesser", '2'))
                            .AddKeybind(new Keybind(() => this._option = new OptionMoneyTracker(), "Money Tracker", '3'))
                            .AddSpacer()
                            .AddKeybind(new Keybind(() => this._running = false, "Quit", key: ConsoleKey.Escape))
                            .Request();
                    }

                    switch (Util.LastInput.Key)
                    {
                        case ConsoleKey.F12: Program.ToggleDebugMode(); break;
                        case ConsoleKey.F11: Directory.Delete(Program.DirectoryPath, true); break;
                    }
                }
                catch (Exception e)
                {
                    if (this._option != null)
                    {
                        this._option.Save();
                        this._option = null;
                    }

                    Util.SetConsoleSize(140, 30);
                    Util.Print(e.ToString());
                    Util.WaitForKey(ConsoleKey.F1);
                    Console.Clear();
                }
            }
        }

        public static void ToggleDebugMode()
        {
            Util.ToggleBool(ref Program._debugMode);
            Console.Clear();
        }

        public static void Main() { new Program().Start(); }
    }
}
