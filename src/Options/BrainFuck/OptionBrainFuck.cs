using B.Inputs;
using B.Utils;

namespace B.Options.BrainFuck
{
    public sealed class OptionBrainFuck : Option
    {
        private const string FILE_EXTENSION = "*bf";

        private static readonly string DirectoryPath = Program.DataPath + @"brainfuck\";

        public BrainFuckProgram CurrentProgram => this._programs[Input.ScrollIndex];

        private readonly Utils.List<BrainFuckProgram> _programs = new();
        private Stage _stage = Stage.MainMenu;

        // Memory Cells for BrainFuck Program.
        private readonly byte[] _memory = new byte[byte.MaxValue];
        // BrainFuck Program Output
        private string _output = string.Empty;
        // Current Cell Index
        private byte _memoryIndex = 0;
        // Depth of Bracket Loops
        private uint _bracketDepth = 0;

        public OptionBrainFuck()
        {
            if (!Directory.Exists(OptionBrainFuck.DirectoryPath))
                Directory.CreateDirectory(OptionBrainFuck.DirectoryPath);
            else
                foreach (string filePath in Directory.GetFiles(OptionBrainFuck.DirectoryPath, OptionBrainFuck.FILE_EXTENSION))
                    this._programs.Add(new BrainFuckProgram(Path.GetFileNameWithoutExtension(filePath), filePath));
        }

        public override void Loop()
        {
            switch (this._stage)
            {
                case Stage.MainMenu:
                    {
                        Util.ClearConsole(20, 7);
                        new Input.Option("BrainFuck")
                            .Add(() => this._stage = Stage.List, "List", '1')
                            .AddExit(this)
                            .Request();
                    }
                    break;

                case Stage.List:
                    {
                        Util.ClearConsole(40, 8 + this._programs.Length);
                        Input.RequestScroll(this._programs.Items, program => program.Title, exitKeybind:
                            new(() =>
                            {
                                Input.ScrollIndex = 0;
                                this._stage = Stage.MainMenu;
                            }, "Back", key: ConsoleKey.Escape),
                            extraKeybinds:
                            new Keybind(() =>
                            {
                                this.Reset();
                                this._stage = Stage.Run;
                            }, "Run", key: ConsoleKey.Enter));
                    }
                    break;

                case Stage.Run:
                    {
                        int totalSteps = 0;
                        BrainFuckProgram program = this.CurrentProgram;
                        Util.ClearConsole(50, 25);

                        for (uint i = 0; i < program.Instructions.Length; i++)
                        {
                            totalSteps++;
                            program.HandleStep(in this._memory, ref this._memoryIndex, ref i, ref this._bracketDepth, ref this._output);
                        }

                        if (this._stage != Stage.List)
                        {
                            Util.PrintLine();
                            Util.PrintLine(this._output);
                            Util.WaitForKey(ConsoleKey.F1);
                            this._stage = Stage.List;
                        }
                    }
                    break;
            }
        }

        // TODO inline if only referenced once
        private void Reset()
        {
            Array.Fill(this._memory, (byte)0);
            this._output = string.Empty;
            this._memoryIndex = 0;
            this._bracketDepth = 0;
        }

        private enum Stage
        {
            MainMenu,
            List,
            Run,
        }
    }
}
