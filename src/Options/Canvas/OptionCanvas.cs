using B.Inputs;
using B.Utils;

namespace B.Options.Canvas
{
    public sealed class OptionCanvas : Option<OptionCanvas.Stages>
    {
        public static Vector2 CANVAS_BORDER_PAD => new(2, 1);

        private const int MAX_INPUT_LENGTH = 25;
        private const int MAX_CANVASES_PER_PAGE = 10;

        private static Vector2 CANVAS_SIZE_MIN => new(20, 10);
        private static Vector2 CURSOR_POS_MIN => new(0, 2);

        public static readonly string DirectoryPath = Program.DataPath + @"canvas\";

        private (int x, int y, int width, int height) _lastConsoleWindow = new(0, 0, 0, 0);
        private Utils.List<CanvasInfo> _canvases = new();
        private CanvasInfo _canvas = null!;
        private ConsoleColor _color = ConsoleColor.Black;
        private Vector2 _pos = Vector2.Zero;

        public OptionCanvas() : base(Stages.MainMenu)
        {
            if (!Directory.Exists(OptionCanvas.DirectoryPath))
                Directory.CreateDirectory(OptionCanvas.DirectoryPath);
            else
                foreach (string filePath in Directory.GetFiles(OptionCanvas.DirectoryPath))
                    this._canvases.Add(Util.Deserialize<CanvasInfo>(filePath));
        }

        public override void Loop()
        {
            switch (this.Stage)
            {
                case Stages.MainMenu:
                    {
                        int consoleHeight = 7;

                        if (!this._canvases.IsEmpty)
                            consoleHeight++;

                        Util.ClearAndSetSize(20, consoleHeight);

                        Input.Choice iob = new Input.Choice("Canvas")
                            .Add(() =>
                            {
                                Util.Clear();
                                this._canvas = new();
                                Input.String = string.Empty;
                                this.Stage = Stages.Create_Name;
                            }, "Create", '1');

                        if (!this._canvases.IsEmpty)
                        {
                            iob.Add(() =>
                            {
                                Input.ScrollIndex = 0;
                                this.Stage = Stages.List;
                            }, "List", '2');
                        }

                        iob.AddExit(this)
                            .Request();
                    }
                    break;

                case Stages.List:
                    {
                        Util.ClearAndSetSize(32, this._canvases.Length + 10);
                        Input.RequestScroll(
                            items: this._canvases.Items,
                            getText: canvas => canvas.Title,
                            title: "Canvases",
                            maxEntriesPerPage: OptionCanvas.MAX_CANVASES_PER_PAGE,
                            exitKeybind: new(() => this.Stage = Stages.MainMenu, key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[] {
                                new(() => SetCanvasAndChangeStage(Stages.View), "View", key: ConsoleKey.Enter),
                                new(() => SetCanvasAndChangeStage(Stages.Edit), "Edit", key: ConsoleKey.Tab),
                                new(() => SetCanvasAndChangeStage(Stages.Delete), "Delete", key: ConsoleKey.Delete)
                            });

                        void SetCanvasAndChangeStage(Stages stage)
                        {
                            this._canvas = this._canvases[Input.ScrollIndex];
                            this.Stage = stage;
                        }
                    }
                    break;

                case Stages.Delete:
                    {
                        Util.ClearAndSetSize(39, 7);
                        new Input.Choice($"Delete '{this._canvas.Title}'?")
                            .Add(() =>
                            {
                                File.Delete(this._canvas.FilePath);
                                this._canvases.Remove(this._canvas);

                                if (this._canvases.IsEmpty)
                                    this.Stage = Stages.MainMenu;
                            }, "yes", key: ConsoleKey.Enter)
                            .AddSpacer()
                            .Add(null!, "NO", key: ConsoleKey.Escape)
                            .Request();
                        this.Stage = Stages.List;
                    }
                    break;

                case Stages.View:
                    {
                        this._canvas.Draw();
                        Util.WaitForKey(ConsoleKey.Escape, true);
                        this.Stage = Stages.List;
                    }
                    break;

                case Stages.Create_Name: this.ShowCreationStage(CreationStage.Name); break;

                case Stages.Create_Size_Height: this.ShowCreationStage(CreationStage.Height); break;

                case Stages.Create_Size_Width: this.ShowCreationStage(CreationStage.Width); break;

                case Stages.Edit:
                    {
                        (int x, int y, int width, int height) consoleWindow = new(Console.WindowLeft, Console.WindowTop, Console.WindowWidth, Console.WindowHeight);
                        Vector2 windowSize = this._canvas.Size + OptionCanvas.CURSOR_POS_MIN + (OptionCanvas.CANVAS_BORDER_PAD * 2);

                        if (consoleWindow != this._lastConsoleWindow)
                        {
                            this._lastConsoleWindow = consoleWindow;
                            this._canvas.Draw(OptionCanvas.CURSOR_POS_MIN);
                        }

                        Util.ResetTextCursor();
                        Util.PrintLine($" Color: {this._color,-10}");
                        Util.PrintLine($" {Util.StringOf('-', windowSize.x - 2)}");
                        Util.SetTextCursorPosition(this._pos + OptionCanvas.CURSOR_POS_MIN + OptionCanvas.CANVAS_BORDER_PAD);
                        new Input.Choice()
                            .Add(() => Move(Direction.Up), key: ConsoleKey.UpArrow)
                            .Add(() => Move(Direction.Down), key: ConsoleKey.DownArrow)
                            .Add(() => Move(Direction.Left), key: ConsoleKey.LeftArrow)
                            .Add(() => Move(Direction.Right), key: ConsoleKey.RightArrow)
                            .Add(() => Paint(), key: ConsoleKey.Spacebar)
                            .Add(() => PaintDirection(Direction.Up), keyChar: '8')
                            .Add(() => PaintDirection(Direction.Down), keyChar: '2')
                            .Add(() => PaintDirection(Direction.Left), keyChar: '4')
                            .Add(() => PaintDirection(Direction.Right), keyChar: '6')
                            .Add(() =>
                            {
                                if (!this._canvases.Contains(this._canvas))
                                    this._canvases.Add(this._canvas);

                                this.Save();
                                this._lastConsoleWindow = new(0, 0, 0, 0);
                                this.Stage = Stages.MainMenu;
                            }, key: ConsoleKey.Escape)
                            .Add(() =>
                            {
                                this._lastConsoleWindow = new(0, 0, 0, 0);
                                this.Stage = Stages.ColorSelect;
                            }, key: ConsoleKey.F1)
                            .Request();

                        void Move(Direction direction)
                        {
                            switch (direction)
                            {
                                case Direction.Up: this._pos.y--; break;
                                case Direction.Down: this._pos.y++; break;
                                case Direction.Left: this._pos.x--; break;
                                // default: Direction.Right
                                default: this._pos.x++; break;
                            }

                            this._pos = Vector2.Clamp(this._pos, Vector2.Zero, this._canvas.Size - Vector2.One);
                            Util.SetTextCursorPosition(this._pos + OptionCanvas.CURSOR_POS_MIN + OptionCanvas.CANVAS_BORDER_PAD);
                        }

                        void Paint()
                        {
                            this._canvas.Color(this._pos) = this._color;
                            Util.Print(' ', colorBackground: this._color);
                        }

                        void PaintDirection(Direction direction)
                        {
                            Move(direction);
                            Paint();
                        }
                    }
                    break;

                case Stages.ColorSelect:
                    {
                        Util.ClearAndSetSize(32, 26);
                        Util.PrintLine();
                        Util.PrintLine("  Color Select");
                        Util.PrintLine();
                        ConsoleColor[] colors = Util.OrderedConsoleColors;
                        Input.RequestScroll(
                            items: colors,
                            getText: c => $" {c.ToString(),-12}",
                            getTextColor: c => c == ConsoleColor.Black || c.ToString().StartsWith("Dark") ? ConsoleColor.White : ConsoleColor.Black,
                            getBackgroundColor: c => c,
                            scrollType: Input.ScrollType.Side,
                            exitKeybind: new Keybind(() => this.Stage = Stages.Edit, "Back", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind(() =>
                            {
                                this._color = colors[Input.ScrollIndex];
                                Input.ScrollIndex = 0;
                                this.Stage = Stages.Edit;
                            }, "Select", key: ConsoleKey.Enter)
                        );
                    }
                    break;
            }
        }

        public override void Save() => Util.Serialize(this._canvas.FilePath, this._canvas);

        private void ShowCreationStage(CreationStage stage)
        {
            Util.ClearAndSetSize(35, 8);
            Util.PrintLine();
            Util.PrintLine("  New Canvas");
            Util.PrintLine();

            if (stage == 0)
                Util.Print($"  Name: {Input.String}");
            else
            {
                Util.PrintLine($"  Name: {this._canvas.Title}");
                Util.PrintLine();
            }

            if (stage == CreationStage.Height)
                Util.Print($"  Height: {Input.String}");
            else if (stage == CreationStage.Width)
                Util.PrintLine($"  Height: {this._canvas.Colors.Length}");

            if (stage == CreationStage.Width)
                Util.Print($"  Width: {Input.String}");

            switch (Input.RequestLine(OptionCanvas.MAX_INPUT_LENGTH).Key)
            {
                case ConsoleKey.Enter:
                    {
                        switch (stage)
                        {
                            case CreationStage.Name:
                                {
                                    if (!string.IsNullOrWhiteSpace(Input.String) && !this._canvases.GetSubList(c => c.Title).Contains(Input.String))
                                    {
                                        this._canvas.Title = Input.String.Trim();
                                        Input.String = string.Empty;
                                        this.Stage = Stages.Create_Size_Height;
                                    }
                                }
                                break;

                            case CreationStage.Height:
                                {
                                    int? height = Input.Int;

                                    if (height.HasValue && height.Value >= OptionCanvas.CANVAS_SIZE_MIN.y)
                                    {
                                        this._canvas.Colors = new ConsoleColor[height.Value][];
                                        Input.String = string.Empty;
                                        this.Stage = Stages.Create_Size_Width;
                                    }
                                }
                                break;

                            case CreationStage.Width:
                                {
                                    int? width = Input.Int;

                                    if (width.HasValue && width.Value >= OptionCanvas.CANVAS_SIZE_MIN.x)
                                    {
                                        for (int i = 0; i < this._canvas.Colors.Length; i++)
                                        {
                                            ConsoleColor[] row = new ConsoleColor[width.Value];
                                            Array.Fill(row, ConsoleColor.White);
                                            this._canvas.Colors[i] = row;
                                        }

                                        Input.String = string.Empty;
                                        Util.Clear();
                                        this.Stage = Stages.Edit;
                                    }
                                }
                                break;
                        }
                    }
                    break;

                case ConsoleKey.Escape:
                    {
                        switch (stage)
                        {
                            case CreationStage.Name:
                                {
                                    this._canvas = null!;
                                    Input.String = string.Empty;
                                    this.Stage = Stages.MainMenu;
                                }
                                break;

                            case CreationStage.Height:
                                {
                                    Input.String = this._canvas.Title;
                                    this.Stage = Stages.Create_Name;
                                }
                                break;

                            case CreationStage.Width:
                                {
                                    Input.String = this._canvas.Colors.Length.ToString();
                                    this.Stage = Stages.Create_Size_Height;
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        private enum CreationStage
        {
            Name,
            Height,
            Width,
        }

        public enum Stages
        {
            MainMenu,
            List,
            Delete,
            View,
            Create_Name,
            Create_Size_Height,
            Create_Size_Width,
            Edit,
            ColorSelect,
        }
    }
}
