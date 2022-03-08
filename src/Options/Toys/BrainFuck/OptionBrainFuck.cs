using B.Inputs;
using B.Utils;

namespace B.Options.Toys.BrainFuck
{
    public sealed class OptionBrainFuck : Option<OptionBrainFuck.Stages>
    {
        private const string FILE_EXTENSION = "*bf";
        private const int MAX_MEMORY_VIEW_LENGTH = 20;

        public static readonly string DirectoryPath = Program.DataPath + @"brainfuck\";
        public static string Title => Program.Settings.Censor.Active ? "BrainF**k" : "BrainFuck";

        private BrainFuckProgram[] _programs = new BrainFuckProgram[0];
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
                    _programs = _programs.Add(new BrainFuckProgram(Path.GetFileNameWithoutExtension(filePath), filePath));
        }

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.ClearAndSetSize(20, 7);
                        Input.CreateChoice(OptionBrainFuck.Title)
                            .Add(() => SetStage(Stages.List), "List", '1')
                            .AddExit(this)
                            .Request();
                    }
                    break;

                case Stages.List:
                    {
                        Window.ClearAndSetSize(40, 10 + _programs.Length);
                        Input.RequestScroll(
                            items: _programs,
                            getText: program => program.Title,
                            title: $"{OptionBrainFuck.Title} Programs",
                            exitKeybind: new(() =>
                            {
                                Input.ScrollIndex = 0;
                                SetStage(Stages.MainMenu);
                            }, "Back", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind(() =>
                            {
                                _currentProgram = _programs[Input.ScrollIndex];
                                Array.Fill(_memory, (byte)0);
                                _output = string.Empty;
                                _instructionIndex = 0;
                                _memoryIndex = 0;
                                _bracketDepth = 0;
                                _stepCounter = 0;
                                SetStage(Stages.Run);
                            }, "Run", key: ConsoleKey.Enter));
                    }
                    break;

                case Stages.Run:
                    {
                        if (_instructionIndex < _currentProgram.Instructions.Length)
                        {
                            if (Program.Settings.DebugMode.Active)
                            {
                                int consoleWidth = 50;
                                Window.ClearAndSetSize(consoleWidth, 25);
                                Window.PrintLine(_output);
                                Window.PrintLine($" {Util.StringOf('-', consoleWidth - 2)}");
                                Window.PrintLine("  DEBUG ON | OUTPUT ABOVE LINE");
                                Window.PrintLine($"Memory Index: {_memoryIndex}");
                                Window.PrintLine($"Instruction Index: {_instructionIndex}");
                                Window.PrintLine($"Bracket Depth: {_bracketDepth}");
                                Window.PrintLine($"Total Steps: {_stepCounter}");

                                switch (Input.Get().Key)
                                {
                                    case ConsoleKey.F1:
                                        {
                                            Window.Clear();
                                            SetStage(Stages.MemoryView);
                                        }; break;

                                    case ConsoleKey.Escape: SetStage(Stages.List); break;
                                }
                            }

                            // Program will run step if Memory View is not pressed in Debug Mode
                            // so that the memory can be viewed from the current step.
                            if (Stage != Stages.MemoryView)
                                HandleStep();
                        }
                        else
                        {
                            Window.ClearAndSetSize(50, 25);
                            Window.PrintLine();
                            Window.PrintLine(_output);
                            Input.WaitFor(ConsoleKey.F1);
                            Input.ScrollIndex = 0;
                            SetStage(Stages.List);
                        }
                    }
                    break;

                case Stages.MemoryView:
                    {
                        if (_instructionIndex < _currentProgram.Instructions.Length)
                        {
                            // TODO add to bottom/top of window, display the instructions with the current instruction highlighted and centered

                            int consoleWidth = 20;
                            Window.SetSize(consoleWidth, 29);
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
                            Input.ScrollIndex = (int)_memoryIndex;
                            Input.RequestScroll(
                                items: _memory,
                                getText: b => string.Format(format, b, ((char)b).Unformat(), $"{b,2:X}"),
                                maxEntriesPerPage: OptionBrainFuck.MAX_MEMORY_VIEW_LENGTH,
                                navigationKeybinds: false,
                                extraKeybinds: new Keybind[] {
                                    new(() => HandleStep(), "Step", key: ConsoleKey.Spacebar),
                                    new(() => SetStage(Stages.Run), "Back", key: ConsoleKey.F1)});
                        }
                        else
                            SetStage(Stages.Run);
                    }
                    break;
            }
        }

        private void HandleStep()
        {
            _currentProgram.HandleStep(in _memory, ref _memoryIndex, ref _instructionIndex, ref _bracketDepth, ref _output);
            _instructionIndex++;
            _stepCounter++;
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
