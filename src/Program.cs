using System.Runtime.InteropServices;
using B.Inputs;
using B.Options;
using B.Options.Adventure;
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
    public sealed class Program : Option<Util.NoEnum>
    {
        private const string USER32 = "user32.dll";
        private const string KERNEL32 = "kernel32.dll";

        // Code entry point
        public static int Main() => new Program().Start();

        // Program Info
        public static ProgramSettings Settings { get; private set; } = new();
        public static string DataPath => Environment.CurrentDirectory + @"\data\";

        // Private Variables
        private readonly (Type, Func<string>)[] _options = new (Type, Func<string>)[] {
            new(typeof(OptionAdventure), () => "Adventure!"),
            new(typeof(OptionBrainFuck), () => OptionBrainFuck.Title),
            new(typeof(OptionCanvas), () => "Canvas"),
            new(typeof(OptionExpressionSolver), () => "Expression Solver"),
            new(typeof(OptionFTP), () => "FTP"),
            new(typeof(OptionMoneyTracker), () => "Money Tracker"),
            new(typeof(OptionNumberGuesser), () => "Number Guesser")};
        private IOption _option = null!;

        public Program() : base(default) { }

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
            // Copied Code to disable some window resizing functionality
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/1aa43c6c-71b9-42d4-aa00-60058a85f0eb/c-console-window-disable-resize?forum=csharpgeneral
            IntPtr handle = Program.GetConsoleWindow();

            if (handle != IntPtr.Zero)
            {
                IntPtr sysMenu = Program.GetSystemMenu(handle, false);

                foreach (int sc in new int[] {
                    0xF000, // SC_SIZE
                    0xF020, // SC_MINIMIZE
                    0xF030, // SC_MAXIMIZE
                    0xF060, // SC_CLOSE
                }) { Program.DeleteMenu(sysMenu, sc, 0x00000000); }
            }

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
        }

        public override void Loop()
        {
            // If directory doesn't exist, create it and add hidden attribute
            if (!Directory.Exists(Program.DataPath))
            {
                DirectoryInfo mainDirectory = Directory.CreateDirectory(Program.DataPath);
                mainDirectory.Attributes = FileAttributes.Hidden;
            }

            if (this._option != null && this._option.IsRunning())
                this._option.Loop();
            else
            {
                // Display main menu options
                Window.ClearAndSetSize(22, this._options.Length + 7);
                Input.Choice iob = new("B's");

                for (int i = 0; i < this._options.Length; i++)
                {
                    (Type, Func<String>) optionEntry = this._options[i];
                    iob.Add(CreateOptionKeybind(optionEntry.Item1, optionEntry.Item2(), (char)('1' + i)));
                }

                iob.AddSpacer()
                    .Add(CreateOptionKeybind(typeof(OptionSettings), "Settings", '9'))
                    .AddExit(this, false)
                    .Request();

                Keybind CreateOptionKeybind(Type optionType, string title, char num) => new Keybind(() => this._option = (IOption)Activator.CreateInstance(optionType)!, title, num);
            }
        }

        private void HandleException(Exception e)
        {
            Window.ClearAndSetSize(Window.SIZE_MAX);
            Window.PrintLine();
            Window.PrintLine("  An exception was thrown!", colorText: ConsoleColor.Red);
            Window.PrintLine();
            Window.PrintLine($"  {e.ToString()}", colorText: ConsoleColor.White, colorBackground: ConsoleColor.Black);
            Input.WaitFor(ConsoleKey.F1);
            Window.Clear();
            this._option = null!;
        }

        [DllImport(Program.USER32)] public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);
        [DllImport(Program.USER32)] private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport(Program.KERNEL32, ExactSpelling = true)] private static extern IntPtr GetConsoleWindow();
    }
}
