using B.Inputs;
using B.Utils;

namespace B.Options.Toys.Canvas
{
    public sealed class OptionCanvas : Option<OptionCanvas.Stages>
    {
        public const string Title = "Canvas";

        public static Vector2 CANVAS_BORDER_PAD => new(2, 1);

        private const int MAX_INPUT_LENGTH = 25;
        private const int MAX_CANVASES_PER_PAGE = 10;

        private static Vector2 CANVAS_SIZE_MIN => new(20, 10);
        private static Vector2 CURSOR_POS_MIN => new(0, 2);

        public static readonly string DirectoryPath = Program.DataPath + @"canvas\";

        private (int x, int y, int width, int height) _lastConsoleWindow = new(0, 0, 0, 0); // TODO replace with better way to see when screen needs to be fully reprinted
        private CanvasInfo[] _canvases = new CanvasInfo[0];
        private CanvasInfo _canvas = null!;
        private ConsoleColor _color = ConsoleColor.Black;
        private Vector2 BrushSize = Vector2.One;
        private Vector2 CursorPos = Vector2.Zero;

        public OptionCanvas() : base(Stages.MainMenu)
        {
            if (!Directory.Exists(OptionCanvas.DirectoryPath))
                Directory.CreateDirectory(OptionCanvas.DirectoryPath);
            else
                foreach (string filePath in Directory.GetFiles(OptionCanvas.DirectoryPath))
                    _canvases = _canvases.Add(Util.Deserialize<CanvasInfo>(filePath));
        }

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        int consoleHeight = 7;

                        if (!_canvases.IsEmpty())
                            consoleHeight++;

                        Window.ClearAndSetSize(20, consoleHeight);

                        Input.Choice iob = Input.CreateChoice(OptionCanvas.Title)
                            .Add(() =>
                            {
                                Window.Clear();
                                _canvas = new();
                                Input.ResetString();
                                SetStage(Stages.Create_Name);
                            }, "Create", '1');

                        if (!_canvases.IsEmpty())
                            iob.Add(() =>
                            {
                                Input.ScrollIndex = 0;
                                SetStage(Stages.List);
                            }, "List", '2');

                        iob.AddExit(this)
                            .Request();
                    }
                    break;

                case Stages.List:
                    {
                        Window.ClearAndSetSize(32, _canvases.Length + 10);
                        Input.RequestScroll(
                            items: _canvases,
                            getText: canvas => canvas.Title,
                            title: "Canvases",
                            maxEntriesPerPage: OptionCanvas.MAX_CANVASES_PER_PAGE,
                            exitKeybind: new(() => SetStage(Stages.MainMenu), key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[] {
                                new(() => SetCanvasAndChangeStage(Stages.View), "View", key: ConsoleKey.Enter),
                                new(() => SetCanvasAndChangeStage(Stages.Edit), "Edit", key: ConsoleKey.Tab),
                                new(() => SetCanvasAndChangeStage(Stages.Delete), "Delete", key: ConsoleKey.Delete)
                            });

                        void SetCanvasAndChangeStage(Stages stage)
                        {
                            _canvas = _canvases[Input.ScrollIndex];
                            Input.ScrollIndex = 0;
                            SetStage(stage);
                        }
                    }
                    break;

                case Stages.Delete:
                    {
                        Window.ClearAndSetSize(39, 7);
                        Input.CreateChoice($"Delete '{_canvas.Title}'?")
                            .Add(() =>
                            {
                                File.Delete(_canvas.FilePath);
                                _canvases = _canvases.Remove(_canvas);

                                if (_canvases.IsEmpty())
                                    SetStage(Stages.MainMenu);
                            }, "yes", key: ConsoleKey.Enter)
                            .AddSpacer()
                            .Add(null!, "NO", key: ConsoleKey.Escape)
                            .Request();
                        SetStage(Stages.List);
                    }
                    break;

                case Stages.View:
                    {
                        _canvas.Draw();
                        Input.WaitFor(ConsoleKey.Escape, true);
                        SetStage(Stages.List);
                    }
                    break;

                case Stages.Create_Name: ShowCreationStage(CreationStage.Name); break;

                case Stages.Create_Size_Height: ShowCreationStage(CreationStage.Height); break;

                case Stages.Create_Size_Width: ShowCreationStage(CreationStage.Width); break;

                case Stages.Edit:
                    {
                        (int x, int y, int width, int height) consoleWindow = new(Console.WindowLeft, Console.WindowTop, Console.WindowWidth, Console.WindowHeight);
                        Vector2 windowSize = _canvas.Size + OptionCanvas.CURSOR_POS_MIN + (OptionCanvas.CANVAS_BORDER_PAD * 2);

                        if (consoleWindow != _lastConsoleWindow)
                        {
                            _lastConsoleWindow = consoleWindow;
                            _canvas.Draw(OptionCanvas.CURSOR_POS_MIN);
                        }

                        Cursor.Reset();
                        Window.PrintLine(string.Format(" {0,-" + (windowSize.x - 2) + "}", $"Brush: {BrushSize} | Color: {_color}"));
                        Window.PrintLine($" {Util.StringOf('-', windowSize.x - 2)}");
                        Cursor.SetPosition(CursorPos + OptionCanvas.CURSOR_POS_MIN + OptionCanvas.CANVAS_BORDER_PAD);
                        Input.CreateChoice()
                            // Move in Direction
                            .Add(() => MoveCursor(Direction.Up), key: ConsoleKey.UpArrow)
                            .Add(() => MoveCursor(Direction.Down), key: ConsoleKey.DownArrow)
                            .Add(() => MoveCursor(Direction.Left), key: ConsoleKey.LeftArrow)
                            .Add(() => MoveCursor(Direction.Right), key: ConsoleKey.RightArrow)
                            // Paint in Direction
                            .Add(() => PaintDirection(Direction.Up), keyChar: '8')
                            .Add(() => PaintDirection(Direction.Down), keyChar: '2')
                            .Add(() => PaintDirection(Direction.Left), keyChar: '4')
                            .Add(() => PaintDirection(Direction.Right), keyChar: '6')
                            // Paint
                            .Add(() => Paint(), key: ConsoleKey.Spacebar)
                            .Add(() => ResizeBrush(Direction.Up), key: ConsoleKey.UpArrow, control: true)
                            .Add(() => ResizeBrush(Direction.Down), key: ConsoleKey.DownArrow, control: true)
                            .Add(() => ResizeBrush(Direction.Left), key: ConsoleKey.LeftArrow, control: true)
                            .Add(() => ResizeBrush(Direction.Right), key: ConsoleKey.RightArrow, control: true)
                            // Color Select
                            .Add(() =>
                            {
                                _lastConsoleWindow = new(0, 0, 0, 0);
                                SetStage(Stages.ColorSelect);
                            }, key: ConsoleKey.F1)
                            // Exit
                            .Add(() =>
                            {
                                if (!_canvases.Contains(_canvas))
                                    _canvases = _canvases.Add(_canvas);

                                Save();
                                _lastConsoleWindow = new(0, 0, 0, 0);
                                SetStage(Stages.MainMenu);
                            }, key: ConsoleKey.Escape)
                            .Request();

                        Vector2 maxCanvasPos = _canvas.Size - Vector2.One;
                        // Fix cursor position
                        CursorPos = CursorPos.Clamp(Vector2.Zero, maxCanvasPos);
                        // Set cursor position
                        Cursor.SetPosition(CursorPos + OptionCanvas.CURSOR_POS_MIN + OptionCanvas.CANVAS_BORDER_PAD);
                        // Fix brush size
                        BrushSize = BrushSize.Clamp(Vector2.One, maxCanvasPos);

                        void Paint()
                        {
                            for (int y = 0; y < BrushSize.y; y++)
                            {
                                for (int x = 0; x < BrushSize.x; x++)
                                {
                                    Vector2 pos = CursorPos + new Vector2(x, y);
                                    _canvas.Color(pos) = _color;
                                    Cursor.SetPosition(pos + OptionCanvas.CURSOR_POS_MIN + OptionCanvas.CANVAS_BORDER_PAD);
                                    Window.Print(' ', colorBG: _color);
                                }
                            }

                            // TODO test if this is needed. may be necessary to keep cursor in same position
                            Cursor.SetPosition(CursorPos);
                        }

                        void PaintDirection(Direction direction)
                        {
                            MoveCursor(direction);
                            Paint();
                        }

                        void ResizeBrush(Direction direction) => MoveVec(ref BrushSize, direction);

                        void MoveCursor(Direction direction) => MoveVec(ref CursorPos, direction);

                        void MoveVec(ref Vector2 vec, Direction direction)
                        {
                            Vector2 dirVec;

                            switch (direction)
                            {
                                // Since coordinates start from top-left corner, up and down are flipped to move appropriately.
                                case Direction.Up: dirVec = Vector2.Down; break; // Move up (y - 1 or Vector2.Down)
                                case Direction.Down: dirVec = Vector2.Up; break; // Move down (y + 1 or Vector2.Up)
                                default: dirVec = (Vector2)direction; break;
                            }

                            vec.Move(dirVec);
                        }
                    }
                    break;

                case Stages.ColorSelect:
                    {
                        Window.ClearAndSetSize(32, 26);
                        ConsoleColor[] colors = Util.OrderedConsoleColors;
                        // TODO once RequestScroll uses Cursor Positioning instead of printlining, adjust cursor position so that the title lines up with the color (4, 1)?
                        Input.RequestScroll(
                            items: colors,
                            getText: c => $" {c.ToString(),-12}",
                            getTextColor: c => c == ConsoleColor.Black || c.ToString().StartsWith("Dark") ? ConsoleColor.White : ConsoleColor.Black,
                            getBackgroundColor: c => c,
                            title: "Color Select",
                            exitKeybind: new Keybind(() => SetStage(Stages.Edit), "Back", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind(() =>
                            {
                                _color = colors[Input.ScrollIndex];
                                Input.ScrollIndex = 0;
                                SetStage(Stages.Edit);
                            }, "Select", key: ConsoleKey.Enter)
                        );
                    }
                    break;
            }
        }

        public override void Save() => Util.Serialize(_canvas.FilePath, _canvas);

        private void ShowCreationStage(CreationStage stage)
        {
            Window.ClearAndSetSize(35, 8);
            Window.PrintLine();
            Window.PrintLine("  New Canvas");
            Window.PrintLine();

            if (stage == 0)
                Window.Print($"  Name: {Input.String}");
            else
            {
                Window.PrintLine($"  Name: {_canvas.Title}");
                Window.PrintLine();
            }

            if (stage == CreationStage.Height)
                Window.Print($"  Height: {Input.String}");
            else if (stage == CreationStage.Width)
                Window.PrintLine($"  Height: {_canvas.Colors.Length}");

            if (stage == CreationStage.Width)
                Window.Print($"  Width: {Input.String}");

            Input.RequestLine(OptionCanvas.MAX_INPUT_LENGTH,
                new Keybind(() =>
                {
                    switch (stage)
                    {
                        case CreationStage.Name:
                            {
                                if (!string.IsNullOrWhiteSpace(Input.String) && !_canvases.FromEach(c => c.Title).Contains(Input.String))
                                {
                                    _canvas.Title = Input.String.Trim();
                                    Input.ResetString(); ;
                                    SetStage(Stages.Create_Size_Height);
                                }
                            }
                            break;

                        case CreationStage.Height:
                            {
                                int? height = Input.Int;

                                if (height.HasValue && height.Value >= OptionCanvas.CANVAS_SIZE_MIN.y)
                                {
                                    _canvas.Colors = new ConsoleColor[height.Value][];
                                    Input.ResetString(); ;
                                    SetStage(Stages.Create_Size_Width);
                                }
                            }
                            break;

                        case CreationStage.Width:
                            {
                                int? width = Input.Int;

                                if (width.HasValue && width.Value >= OptionCanvas.CANVAS_SIZE_MIN.x)
                                {
                                    for (int i = 0; i < _canvas.Colors.Length; i++)
                                    {
                                        ConsoleColor[] row = new ConsoleColor[width.Value];
                                        Array.Fill(row, ConsoleColor.White);
                                        _canvas.Colors[i] = row;
                                    }

                                    Input.ResetString(); ;
                                    Window.Clear();
                                    SetStage(Stages.Edit);
                                }
                            }
                            break;
                    }
                }, key: ConsoleKey.Enter),
                new Keybind(() =>
                {
                    switch (stage)
                    {
                        case CreationStage.Name:
                            {
                                _canvas = null!;
                                Input.ResetString(); ;
                                SetStage(Stages.MainMenu);
                            }
                            break;

                        case CreationStage.Height:
                            {
                                Input.String = _canvas.Title;
                                SetStage(Stages.Create_Name);
                            }
                            break;

                        case CreationStage.Width:
                            {
                                Input.String = _canvas.Colors.Length.ToString();
                                SetStage(Stages.Create_Size_Height);
                            }
                            break;
                    }
                }, key: ConsoleKey.Escape));
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
