using B.Inputs;
using B.Utils;

namespace B.Options.BrainFuck
{
    public sealed class OptionBrainFuck : Option<OptionBrainFuck.Stages>
    {
        private const string FILE_EXTENSION = "*bf";
        private const int MAX_MEMORY_VIEW_LENGTH = 20;
        private static byte[] MEMORY_VIEW_REMOVE => new byte[] { 7, 8, 9, 10, 13 };

        public static readonly string DirectoryPath = Program.DataPath + @"brainfuck\";

        private readonly Utils.List<BrainFuckProgram> _programs = new();
        private BrainFuckProgram _currentProgram = null!;

        // Memory Cells for BrainFuck Program.
        private readonly byte[] _memory = new byte[short.MaxValue];
        // BrainFuck Program Output
        private string _output = string.Empty;
        // Current Instruction Index
        private uint _instructionIndex = 0;
        // Current Cell Index
        private uint _memoryIndex = 0;
        // Depth of Bracket Loops
        private uint _bracketDepth = 0;
        // Total Step Counter
        private uint _stepCounter = 0;

        public OptionBrainFuck() : base(Stages.MainMenu)
        {
            if (!Directory.Exists(OptionBrainFuck.DirectoryPath))
                Directory.CreateDirectory(OptionBrainFuck.DirectoryPath);
            else
                foreach (string filePath in Directory.GetFiles(OptionBrainFuck.DirectoryPath, OptionBrainFuck.FILE_EXTENSION))
                    this._programs.Add(new BrainFuckProgram(Path.GetFileNameWithoutExtension(filePath), filePath));
        }

        public override void Loop()
        {
            switch (this.Stage)
            {
                case Stages.MainMenu:
                    {
                        Util.ClearConsole(20, 7);
                        new Input.Option("BrainFuck")
                            .Add(() => this.Stage = Stages.List, "List", '1')
                            .AddExit(this)
                            .Request();
                    }
                    break;

                case Stages.List:
                    {
                        Util.ClearConsole(40, 8 + this._programs.Length);
                        Util.PrintLine();
                        Input.RequestScroll(
                            items: this._programs.Items,
                            getText: program => program.Title,
                            exitKeybind: new(() =>
                            {
                                Input.ScrollIndex = 0;
                                this.Stage = Stages.MainMenu;
                            }, "Back", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind(() =>
                            {
                                this._currentProgram = this._programs[Input.ScrollIndex];
                                Array.Fill(this._memory, (byte)0);
                                this._output = string.Empty;
                                this._instructionIndex = 0;
                                this._memoryIndex = 0;
                                this._bracketDepth = 0;
                                this._stepCounter = 0;
                                this.Stage = Stages.Run;
                            }, "Run", key: ConsoleKey.Enter));
                    }
                    break;

                case Stages.Run:
                    {
                        if (this._instructionIndex < this._currentProgram.Instructions.Length)
                        {
                            if (Program.Settings.DebugMode)
                            {
                                int consoleWidth = 50;
                                Util.ClearConsole(consoleWidth, 25);
                                Util.PrintLine(this._output);
                                Util.PrintLine($" {Util.StringOf('-', consoleWidth - 2)}");
                                Util.PrintLine("  DEBUG ON | OUTPUT ABOVE LINE");
                                Util.PrintLine($"Memory Index: {this._memoryIndex}");
                                Util.PrintLine($"Instruction Index: {this._instructionIndex}");
                                Util.PrintLine($"Bracket Depth: {this._bracketDepth}");
                                Util.PrintLine($"Total Steps: {this._stepCounter}");

                                switch (Util.GetKey().Key)
                                {
                                    case ConsoleKey.F1:
                                        {
                                            Util.ClearConsole();
                                            this.Stage = Stages.MemoryView;
                                        }; break;

                                    case ConsoleKey.Escape: this.Stage = Stages.List; break;
                                }
                            }

                            // Program will run step if Memory View is not pressed in Debug Mode
                            // so that the memory can be viewed from the current step.
                            if (this.Stage != Stages.MemoryView)
                                this.HandleStep();
                        }
                        else
                        {
                            Util.ClearConsole(50, 25);
                            Util.PrintLine();
                            Util.PrintLine(this._output);
                            Util.WaitForKey(ConsoleKey.F1);
                            Input.ScrollIndex = 0;
                            this.Stage = Stages.List;
                        }
                    }
                    break;

                case Stages.MemoryView:
                    {
                        if (this._instructionIndex < this._currentProgram.Instructions.Length)
                        {
                            int consoleWidth = 20;
                            Util.SetConsoleSize(consoleWidth, 29);
                            Util.ResetTextCursor();
                            Util.PrintLine();
                            Util.PrintLine("  Memory View");
                            Util.PrintLine();
                            string format = "{0,-6}{1,-6}{2,-2}";
                            Util.PrintLine($"   {string.Format(format, "byte", "char", "hex")}");
                            Util.PrintLine($" {Util.StringOf('-', consoleWidth - 2)}");
                            Input.ScrollIndex = (int)this._memoryIndex;
                            Input.RequestScroll(
                                items: this._memory,
                                getText: b => string.Format(format, b, OptionBrainFuck.MEMORY_VIEW_REMOVE.Contains(b) ? ' ' : (char)b, $"{b,2:X}"),
                                maxEntriesPerPage: OptionBrainFuck.MAX_MEMORY_VIEW_LENGTH,
                                scrollType: Input.ScrollType.Side,
                                navigationKeybinds: false,
                                extraKeybinds: new Keybind[] {
                                    new(() => this.HandleStep(), "Step", key: ConsoleKey.Spacebar),
                                    new(() => this.Stage = Stages.Run, "Back", key: ConsoleKey.F1)});
                        }
                        else
                            this.Stage = Stages.Run;
                    }
                    break;
            }
        }

        private void HandleStep()
        {
            this._currentProgram.HandleStep(in this._memory, ref this._memoryIndex, ref this._instructionIndex, ref this._bracketDepth, ref this._output);
            this._instructionIndex++;
            this._stepCounter++;
        }

        public enum Stages
        {
            MainMenu,
            List,
            Run,
            MemoryView,
        }
    }
}
