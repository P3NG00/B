using B.Inputs;
using B.Utils;
using B.Utils.Enums;
using B.Utils.Extensions;

namespace B.Options.Toys.BrainFuck
{
    public sealed class OptionBrainFuck : Option<OptionBrainFuck.Stages>
    {
        #region TODOs

        // TODO add 'editor' mode

        #endregion



        #region Constants

        private const string FILE_EXTENSION = "*bf";
        private const int MAX_MEMORY_VIEW_LENGTH = 20;

        #endregion



        #region Universal Properties

        public static string DirectoryPath => Program.DataPath + @"brainfuck\";
        public static string Title => Program.Settings.Censor ? "BrainF**k" : "BrainFuck";

        #endregion



        #region Private Variables

        private List<BrainFuckProgram> _programs = new();
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

        #endregion



        #region Constructors

        public OptionBrainFuck() : base(Stages.MainMenu)
        {
            if (!Directory.Exists(OptionBrainFuck.DirectoryPath))
                Directory.CreateDirectory(OptionBrainFuck.DirectoryPath);
            else
                foreach (string filePath in Directory.GetFiles(OptionBrainFuck.DirectoryPath, OptionBrainFuck.FILE_EXTENSION))
                    _programs.Add(new BrainFuckProgram(Path.GetFileNameWithoutExtension(filePath), filePath));
        }

        #endregion



        #region Override Methods

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.SetSize(20, 7);
                        Cursor.Set(0, 1);
                        Choice choice = new(OptionBrainFuck.Title);
                        choice.AddKeybind(Keybind.Create(() =>
                        {
                            Input.ScrollIndex = 0;
                            SetStage(Stages.List);
                        }, "List", '1'));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateOptionExit(this));
                        choice.Request();
                    }
                    break;

                case Stages.List:
                    {
                        Window.SetSize(40, 10 + _programs.Count);
                        Cursor.y = 1;
                        Input.RequestScroll(
                            items: _programs,
                            getText: program => program.Title,
                            title: $"{OptionBrainFuck.Title} Programs",
                            exitKeybind: Keybind.Create(() =>
                            {
                                Input.ScrollIndex = 0;
                                SetStage(Stages.MainMenu);
                            }, "Back", key: ConsoleKey.Escape),
                            extraKeybinds: Keybind.Create(() =>
                            {
                                _currentProgram = _programs[Input.ScrollIndex];
                                Array.Fill(_memory, (byte)0);
                                _output = string.Empty;
                                _instructionIndex = 0;
                                _memoryIndex = 0;
                                _bracketDepth = 0;
                                _stepCounter = 0;
                                SetStage(Stages.Run);
                            }, "Run", key: ConsoleKey.Enter)
                        );
                    }
                    break;

                case Stages.Run:
                    {
                        if (_instructionIndex < _currentProgram.Instructions.Length)
                        {
                            if (Program.Settings.DebugMode)
                            {
                                int consoleWidth = 50;
                                Window.SetSize(consoleWidth, 25);
                                Cursor.Set(0, 0);
                                Window.Print(_output);
                                Cursor.Set(1, 1);
                                Window.Print($"{'-'.Loop(consoleWidth - 2)}");
                                Cursor.Set(2, 2);
                                Window.Print("DEBUG ON | OUTPUT ABOVE LINE");
                                Cursor.Set(0, 3);
                                Window.Print($"Memory Index: {_memoryIndex}");
                                Cursor.Set(0, 4);
                                Window.Print($"Instruction Index: {_instructionIndex}");
                                Cursor.Set(0, 5);
                                Window.Print($"Bracket Depth: {_bracketDepth}");
                                Cursor.Set(0, 6);
                                Window.Print($"Total Steps: {_stepCounter}");

                                switch (Input.Get().Key)
                                {
                                    case ConsoleKey.F1: SetStage(Stages.MemoryView); break;
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
                            Window.Clear();
                            Window.SetSize(50, 25);
                            Cursor.Set(0, 1);
                            Window.Print(_output);
                            Input.WaitFor(ConsoleKey.F1);
                            SetStage(Stages.List);
                        }
                    }
                    break;

                case Stages.MemoryView:
                    {
                        if (_instructionIndex < _currentProgram.Instructions.Length)
                        {
                            Input.ScrollIndex = (int)_memoryIndex;
                            int consoleWidth = 21;
                            int widthBordered = consoleWidth - 2;
                            Window.SetSize(consoleWidth, 31);
                            Cursor.Set(4, 1);
                            Window.Print(" Memory View ", PrintType.Title);
                            // Column 1 - Byte
                            // Column 2 - Char
                            // Column 3 - Hex
                            string edge = '-'.Loop(widthBordered);
                            Cursor.Set(1, 2);
                            Window.Print(edge);
                            Cursor.Set(1, 3);
                            // Print instructions
                            int instructionIndex = (int)_instructionIndex;
                            int start = instructionIndex - (consoleWidth / 2) + 1;
                            int end = start + widthBordered;
                            for (int index = start; index < end; index++)
                            {
                                if (index >= 0 && index < _currentProgram.Instructions.Length)
                                    Window.Print(_currentProgram.Instructions[index], index == instructionIndex ? PrintType.Highlight : PrintType.General);
                                else
                                    Window.Print(' ');
                            }
                            Cursor.Set(1, 4);
                            Window.Print(edge);
                            Cursor.Set(1, 5);
                            string format = "{0,-6}{1,-6}{2,-2}";
                            Window.Print($" {string.Format(format, "byte", "char", "hex")}");
                            Cursor.Set(1, 6);
                            Window.Print(edge);
                            Cursor.Set(0, 7);
                            Input.RequestScroll(
                                items: _memory,
                                getText: b => string.Format(format, b, ((char)b).Unformat(), $"{b,2:X}"),
                                maxEntriesPerPage: OptionBrainFuck.MAX_MEMORY_VIEW_LENGTH,
                                navigationKeybinds: false,
                                extraKeybinds: new Keybind[] {
                                    Keybind.Create(HandleStep, "Step", key: ConsoleKey.Spacebar),
                                    Keybind.Create(() =>
                                    {
                                        Input.ScrollIndex = 0;
                                        SetStage(Stages.Run);
                                    }, "Back", key: ConsoleKey.F1)
                                }
                            );
                        }
                        else
                            SetStage(Stages.Run);
                    }
                    break;
            }
        }

        #endregion



        #region Private Variables

        private void HandleStep()
        {
            _currentProgram.HandleStep(in _memory, ref _memoryIndex, ref _instructionIndex, ref _bracketDepth, ref _output);
            _instructionIndex++;
            _stepCounter++;
        }

        #endregion



        #region Enums

        public enum Stages
        {
            MainMenu,
            List,
            Run,
            MemoryView,
        }

        #endregion
    }
}
