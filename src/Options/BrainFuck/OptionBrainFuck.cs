using B.Inputs;
using B.Utils;

namespace B.Options.BrainFuck
{
    public sealed class OptionBrainFuck : Option<OptionBrainFuck.Stages>
    {
        private const string FILE_EXTENSION = "*bf";
        private const int MAX_MEMORY_VIEW_LENGTH = 20;

        public static readonly string DirectoryPath = Program.DataPath + @"brainfuck\";
        public static string Title => Program.Settings.Censor.Active ? "BrainF**k" : "BrainFuck";

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
                        Window.ClearAndSetSize(20, 7);
                        new Input.Choice(OptionBrainFuck.Title)
                            .Add(() => this.SetStage(Stages.List), "List", '1')
                            .AddExit(this)
                            .Request();
                    }
                    break;

                case Stages.List:
                    {
                        Window.ClearAndSetSize(40, 10 + this._programs.Length);
                        Input.RequestScroll(
                            items: this._programs.Items,
                            getText: program => program.Title,
                            title: $"{OptionBrainFuck.Title} Programs",
                            exitKeybind: new(() =>
                            {
                                Input.ScrollIndex = 0;
                                this.SetStage(Stages.MainMenu);
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
                                this.SetStage(Stages.Run);
                            }, "Run", key: ConsoleKey.Enter));
                    }
                    break;

                case Stages.Run:
                    {
                        if (this._instructionIndex < this._currentProgram.Instructions.Length)
                        {
                            if (Program.Settings.DebugMode.Active)
                            {
                                int consoleWidth = 50;
                                Window.ClearAndSetSize(consoleWidth, 25);
                                Window.PrintLine(this._output);
                                Window.PrintLine($" {Util.StringOf('-', consoleWidth - 2)}");
                                Window.PrintLine("  DEBUG ON | OUTPUT ABOVE LINE");
                                Window.PrintLine($"Memory Index: {this._memoryIndex}");
                                Window.PrintLine($"Instruction Index: {this._instructionIndex}");
                                Window.PrintLine($"Bracket Depth: {this._bracketDepth}");
                                Window.PrintLine($"Total Steps: {this._stepCounter}");

                                switch (Input.Get().Key)
                                {
                                    case ConsoleKey.F1:
                                        {
                                            Window.Clear();
                                            this.SetStage(Stages.MemoryView);
                                        }; break;

                                    case ConsoleKey.Escape: this.SetStage(Stages.List); break;
                                }
                            }

                            // Program will run step if Memory View is not pressed in Debug Mode
                            // so that the memory can be viewed from the current step.
                            if (this.Stage != Stages.MemoryView)
                                this.HandleStep();
                        }
                        else
                        {
                            Window.ClearAndSetSize(50, 25);
                            Window.PrintLine();
                            Window.PrintLine(this._output);
                            Input.WaitFor(ConsoleKey.F1);
                            Input.ScrollIndex = 0;
                            this.SetStage(Stages.List);
                        }
                    }
                    break;

                case Stages.MemoryView:
                    {
                        if (this._instructionIndex < this._currentProgram.Instructions.Length)
                        {
                            // TODO add to bottom/top of window, display the instructions with the current instruction highlighted and centered

                            int consoleWidth = 20;
                            Window.Size = (consoleWidth, 29);
                            Cursor.Reset();
                            Window.PrintLine();
                            Window.PrintLine("  Memory View");
                            Window.PrintLine();
                            // Column 1 - Byte
                            // Column 2 - Char
                            // Column 3 - Hex
                            string format = "{0,-6}{1,-6}{2,-2}";
                            Window.PrintLine($"   {string.Format(format, "byte", "char", "hex")}");
                            Window.PrintLine($" {Util.StringOf('-', consoleWidth - 2)}");
                            Input.ScrollIndex = (int)this._memoryIndex;
                            Input.RequestScroll(
                                items: this._memory,
                                getText: b => string.Format(format, b, Util.Unformat(b), $"{b,2:X}"),
                                maxEntriesPerPage: OptionBrainFuck.MAX_MEMORY_VIEW_LENGTH,
                                scrollType: Input.ScrollType.Side,
                                navigationKeybinds: false,
                                extraKeybinds: new Keybind[] {
                                    new(() => this.HandleStep(), "Step", key: ConsoleKey.Spacebar),
                                    new(() => this.SetStage(Stages.Run), "Back", key: ConsoleKey.F1)});
                        }
                        else
                            this.SetStage(Stages.Run);
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
