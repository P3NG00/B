﻿using B.Inputs;
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
||     2022.01.14    ||
||                   ||
\* ================= */

namespace B
{
    public class Program
    {
        public static string DirectoryPath => Environment.CurrentDirectory + @"\data\";
        public static ProgramSettings Settings { get; private set; } = new ProgramSettings();

        private readonly Dict<string, Type> _optionDict = new Dict<string, Type>(
            new Pair<string, Type>("Adventure!", typeof(OptionAdventure)),
            new Pair<string, Type>("Money Tracker", typeof(OptionMoneyTracker)),
            new Pair<string, Type>("Number Guesser", typeof(OptionNumberGuesser))
        );

        private Option? _option = null;
        private bool _running = true;

        private void Start()
        {
            // Set console window title
            Console.Title = "B";

            // Console input ctrl+c
            Console.TreatControlCAsInput = true;

            // Load program settings
            if (File.Exists(ProgramSettings.SettingsPath))
                Program.Settings = Util.Deserialize<ProgramSettings>(ProgramSettings.SettingsPath);

            // Set console colors
            this.UpdateColors();

            // Program loop
            while (this._running)
            {
                try
                {
                    // If directory doesn't exist, create it and add hidden attribute
                    if (!Directory.Exists(Program.DirectoryPath))
                    {
                        DirectoryInfo mainDirectory = Directory.CreateDirectory(Program.DirectoryPath);
                        mainDirectory.Attributes = FileAttributes.Hidden;
                    }

                    // If option is running, execute option code
                    if (this._option != null && this._option.IsRunning)
                        this._option.Loop();
                    else
                    {
                        // Display main menu options
                        Console.Clear();
                        int consoleHeight = this._optionDict.Length + 6;

                        if (Program.Settings.DebugMode)
                            consoleHeight += 2;

                        Util.SetConsoleSize(20, consoleHeight);

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
                        case ConsoleKey.F11: Directory.Delete(Program.DirectoryPath, true); break;

                        // Key to toggle debug mode
                        case ConsoleKey.F12:
                            {
                                Util.ToggleBool(ref Program.Settings.DebugMode);
                                // When toggling Debug mode, the console should be cleared
                                // in case it affects the visuals to be printed.
                                Console.Clear();
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    // Go back to main menu if exception was caught
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

            // Save before exiting
            Util.Serialize(ProgramSettings.SettingsPath, Program.Settings);
        }

        private void UpdateColors()
        {
            Console.BackgroundColor = Program.Settings.DarkMode ? ConsoleColor.Black : ConsoleColor.White;
            Console.ForegroundColor = Program.Settings.DarkMode ? ConsoleColor.White : ConsoleColor.Black;
        }

        // Code entry point
        public static void Main() => new Program().Start();
    }
}
