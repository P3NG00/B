﻿using System.Runtime.InteropServices;
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
        public static void Main() => new Program().Start();

        public static string DataPath => Environment.CurrentDirectory + @"\data\";
        public static Settings Settings { get; private set; } = new Settings();

        // NEW OPTIONS ONLY NEED TO BE REGISTERED HERE
        private readonly Dict<string, Type> _optionDict = new Dict<string, Type>(
            new Pair<string, Type>("Adventure!", typeof(OptionAdventure)),
            new Pair<string, Type>("FTP", typeof(OptionFTP)),
            new Pair<string, Type>("Money Tracker", typeof(OptionMoneyTracker)),
            new Pair<string, Type>("Number Guesser", typeof(OptionNumberGuesser))
        // , new Pair<string, Type>("DEBUG", typeof(OptionDebug))
        );

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
            if (File.Exists(Settings.Path))
                Program.Settings = Util.Deserialize<Settings>(Settings.Path);

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
                if (this._option != null && this._option.IsRunning)
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

                    Input.Option iob = new Input.Option("B's");

                    for (int i = 0; i < this._optionDict.Length; i++)
                    {
                        Pair<string, Type> optionEntry = this._optionDict[i];
                        iob.AddKeybind(new Keybind(() => this._option = (Activator.CreateInstance(optionEntry.Item2!) as Option)!, optionEntry.Item1!, (char)('1' + i)));
                    }

                    iob.AddSpacer()
                        .AddKeybind(new Keybind(() => this._running = false, "Quit", key: ConsoleKey.Escape))
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
                                .AddKeybind(new Keybind(() => Directory.Delete(Program.DataPath, true), "Yes", key: ConsoleKey.Enter))
                                .AddKeybind(new Keybind(null!, "No", key: ConsoleKey.Escape))
                                .Request();
                            Directory.Delete(Program.DataPath, true);
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

        private void Save() => Util.Serialize(Settings.Path, Program.Settings);

        private void UpdateColors()
        {
            Console.BackgroundColor = Program.Settings.DarkMode ? ConsoleColor.Black : ConsoleColor.White;
            Console.ForegroundColor = Program.Settings.DarkMode ? ConsoleColor.White : ConsoleColor.Black;
        }
    }
}
