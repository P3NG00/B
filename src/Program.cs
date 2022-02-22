﻿using System.Runtime.InteropServices;
using B.Inputs;
using B.Options;
using B.Options.Adventure;
using B.Options.BrainFuck;
using B.Options.Canvas;
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
        public static ProgramSettings Settings { get; private set; } = new();
        public static Vector2 WINDOW_MIN => new(16, 2);
        public static Vector2 WINDOW_MAX => new(Console.LargestWindowWidth, Console.LargestWindowHeight);
        public static string DataPath => Environment.CurrentDirectory + @"\data\";

        // Private Variables
        private readonly (Type, Func<string>)[] _options = new (Type, Func<string>)[] {
            new (typeof(OptionAdventure), () => "Adventure!"),
            new (typeof(OptionBrainFuck), () => OptionBrainFuck.Title),
            new (typeof(OptionCanvas), () => "Canvas"),
            new (typeof(OptionFTP), () => "FTP"),
            new (typeof(OptionMoneyTracker), () => "Money Tracker"),
            new (typeof(OptionNumberGuesser), () => "Number Guesser")};
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
            // Disable drag resizing
            this.DeleteWindowMenu(0xF000);

            // Disable window 'x' close button
            this.DeleteWindowMenu(0xF060);

            // Disable minimize button
            this.DeleteWindowMenu(0xF020);

            // Disable maximize button
            this.DeleteWindowMenu(0xF030);

            Console.Beep();

            // Set console window title
            Console.Title = "B";

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
                Window.ClearAndSetSize(24, this._options.Length + 7);
                Input.Choice iob = new("B's");

                for (int i = 0; i < this._options.Length; i++)
                {
                    var optionEntry = this._options[i];
                    iob.Add(() => this._option = (IOption)Activator.CreateInstance(optionEntry.Item1)!, optionEntry.Item2(), (char)('1' + i));
                }

                iob.AddSpacer()
                    .Add(() => this._option = Activator.CreateInstance<OptionSettings>(), "Settings", '9')
                    .Add(() => this._running = false, "Quit", key: ConsoleKey.Escape)
                    .Request();
            }
        }

        private void HandleException(Exception e)
        {
            Window.ClearAndSetSize(Program.WINDOW_MAX);
            Window.PrintLine();
            Window.PrintLine("  An exception was thrown!", colorText: ConsoleColor.Red);
            Window.PrintLine();
            Window.PrintLine($"  {e.ToString()}", colorText: ConsoleColor.White, colorBackground: ConsoleColor.Black);
            Input.WaitFor(ConsoleKey.F1);
            Window.Clear();
            this._option = null!;
        }

        private void DeleteWindowMenu(int nPosition)
        {
            IntPtr handle = Program.GetConsoleWindow();

            if (handle != IntPtr.Zero)
                Program.DeleteMenu(Program.GetSystemMenu(handle, false), nPosition, 0x00000000);
        }

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
    }
}
