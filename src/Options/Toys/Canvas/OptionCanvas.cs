using B.Inputs;
using B.Utils;
using B.Utils.Extensions;

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
        private List<CanvasInfo> _canvases = new();
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
                    _canvases.Add(Util.Deserialize<CanvasInfo>(filePath));
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

                        Window.Clear();
                        Window.SetSize(20, consoleHeight);

                        Input.Choice choice = Input.Choice.Create(OptionCanvas.Title);
                        choice.Add(() =>
                        {
                            Window.Clear();
                            _canvas = new();
                            Input.ResetString();
                            SetStage(Stages.Create_Name);
                        }, "Create", '1');

                        if (!_canvases.IsEmpty())
                            choice.Add(() =>
                            {
                                Input.ScrollIndex = 0;
                                SetStage(Stages.List);
                            }, "List", '2');

                        choice.AddSpacer();
                        choice.AddExit(this);
                        choice.Request();
                    }
                    break;

                case Stages.List:
                    {
                        Window.Clear();
                        Window.SetSize(32, _canvases.Count + 10);
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
                        Window.Clear();
                        Window.SetSize(39, 7);
                        Input.Choice choice = Input.Choice.Create($"Delete '{_canvas.Title}'?");
                        choice.Add(() =>
                        {
                            File.Delete(_canvas.FilePath);
                            _canvases.Remove(_canvas);

                            if (_canvases.IsEmpty())
                                SetStage(Stages.MainMenu);
                        }, "yes", key: ConsoleKey.Enter);
                        choice.AddSpacer();
                        choice.Add(null!, "NO", key: ConsoleKey.Escape);
                        choice.Request();
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

                case Stages.Create_Name: ShowCreationStage(); break;

                case Stages.Create_Size_Height: ShowCreationStage(); break;

                case Stages.Create_Size_Width: ShowCreationStage(); break;

                case Stages.Edit:
                    {
                        (int x, int y, int width, int height) consoleWindow = new(Console.WindowLeft, Console.WindowTop, Console.WindowWidth, Console.WindowHeight);
                        Vector2 windowSize = _canvas.Size + OptionCanvas.CURSOR_POS_MIN + (OptionCanvas.CANVAS_BORDER_PAD * 2);

                        if (consoleWindow != _lastConsoleWindow)
                        {
                            _lastConsoleWindow = consoleWindow;
                            _canvas.Draw(OptionCanvas.CURSOR_POS_MIN);
                        }

                        Cursor.Position = new(1, 0);
                        Window.Print(string.Format(" {0,-" + (windowSize.x - 2) + "}", $"Brush: {BrushSize} | Color: {_color}"));
                        Cursor.Position = new(1, 1);
                        Window.Print($" {'-'.Loop(windowSize.x - 2)}");
                        Cursor.Position = new(CursorPos + OptionCanvas.CURSOR_POS_MIN + OptionCanvas.CANVAS_BORDER_PAD);
                        Input.Choice choice = Input.Choice.Create();
                        // Move in Direction
                        choice.Add(() => MoveCursor(Direction.Up), keyChar: 'w', key: ConsoleKey.UpArrow);
                        choice.Add(() => MoveCursor(Direction.Down), keyChar: 's', key: ConsoleKey.DownArrow);
                        choice.Add(() => MoveCursor(Direction.Left), keyChar: 'a', key: ConsoleKey.LeftArrow);
                        choice.Add(() => MoveCursor(Direction.Right), keyChar: 'd', key: ConsoleKey.RightArrow);
                        // Paint in Direction
                        choice.Add(() => PaintDirection(Direction.Up), keyChar: '8');
                        choice.Add(() => PaintDirection(Direction.Down), keyChar: '2');
                        choice.Add(() => PaintDirection(Direction.Left), keyChar: '4');
                        choice.Add(() => PaintDirection(Direction.Right), keyChar: '6');
                        // Paint
                        choice.Add(() => Paint(), key: ConsoleKey.Spacebar);
                        // Resize Brush
                        choice.Add(() => ResizeBrush(Direction.Up), key: ConsoleKey.UpArrow, control: true);
                        choice.Add(() => ResizeBrush(Direction.Down), key: ConsoleKey.DownArrow, control: true);
                        choice.Add(() => ResizeBrush(Direction.Left), key: ConsoleKey.LeftArrow, control: true);
                        choice.Add(() => ResizeBrush(Direction.Right), key: ConsoleKey.RightArrow, control: true);
                        // Color Select
                        choice.Add(() =>
                        {
                            _lastConsoleWindow = new(0, 0, 0, 0);
                            SetStage(Stages.ColorSelect);
                        }, key: ConsoleKey.F1);
                        // Exit
                        choice.Add(() =>
                        {
                            if (!_canvases.Contains(_canvas))
                                _canvases.Add(_canvas);

                            Save();
                            _lastConsoleWindow = new(0, 0, 0, 0);
                            SetStage(Stages.MainMenu);
                        }, key: ConsoleKey.Escape);
                        choice.Request();

                        Vector2 maxCanvasPos = _canvas.Size - Vector2.One;
                        // Fix cursor position
                        CursorPos = CursorPos.Clamp(Vector2.Zero, maxCanvasPos);
                        // Set cursor position
                        Cursor.Position = new(CursorPos + OptionCanvas.CURSOR_POS_MIN + OptionCanvas.CANVAS_BORDER_PAD);
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
                                    Cursor.Position = new(pos + OptionCanvas.CURSOR_POS_MIN + OptionCanvas.CANVAS_BORDER_PAD);
                                    Window.Print(' ', colorBG: _color);
                                }
                            }

                            // TODO test if this is needed. may be necessary to keep cursor in same position
                            Cursor.Position = new(CursorPos);
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
                        Window.Clear();
                        Window.SetSize(32, 26);
                        ConsoleColor[] colors = Util.OrderedConsoleColors;
                        // TODO once RequestScroll uses Cursor Positioning instead of printlining, adjust cursor position so that the title lines up with the color (4, 1)?
                        Input.RequestScroll(
                            items: colors,
                            getText: c => $" {c.ToString(),-12}",
                            getTextColor: c => c == ConsoleColor.Black || c.ToString().StartsWith("Dark") ? ConsoleColor.White : ConsoleColor.Black,
                            getBackgroundColor: c => c,
                            title: "Color Select",
                            exitKeybind: new Keybind(() => ExitColorSelect(), "Back", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind(() =>
                            {
                                _color = colors[Input.ScrollIndex];
                                ExitColorSelect();
                            }, "Select", key: ConsoleKey.Enter)
                        );

                        void ExitColorSelect()
                        {
                            Input.ScrollIndex = 0;
                            SetStage(Stages.Edit);
                        }
                    }
                    break;
            }
        }

        public override void Save() => Util.Serialize(_canvas.FilePath, _canvas);

        private void ShowCreationStage()
        {
            Window.Clear();
            Window.SetSize(35, 8);
            Cursor.Position = new(2, 1);
            Window.Print("New Canvas");
            Cursor.Position = new(2, 3);
            Window.Print($"Name: {(Stage == Stages.Create_Name ? Input.String : _canvas.Title)}");

            // TODO change creation stage order Width and Height

            if (Stage != Stages.Create_Name)
            {
                Cursor.Position = new(2, 5);
                string print = string.Empty;

                if (Stage == Stages.Create_Size_Height)
                    print = Input.String;
                else if (Stage == Stages.Create_Size_Width)
                    print = _canvas.Colors.Length.ToString();

                Window.Print(string.Format("Height: {0}", print));
            }

            if (Stage == Stages.Create_Size_Width)
            {
                Cursor.Position = new(2, 6);
                Window.Print($"Width: {Input.String}");
            }

            Input.RequestLine(OptionCanvas.MAX_INPUT_LENGTH,
                new Keybind(() =>
                {
                    switch (Stage)
                    {
                        case Stages.Create_Name:
                            {
                                if (!string.IsNullOrWhiteSpace(Input.String) && !_canvases.FromEach(c => c.Title).Contains(Input.String))
                                {
                                    _canvas.Title = Input.String.Trim();
                                    Input.ResetString();
                                    SetStage(Stages.Create_Size_Height);
                                }
                            }
                            break;

                        case Stages.Create_Size_Height:
                            {
                                int? height = Input.Int;

                                if (height.HasValue && height.Value >= OptionCanvas.CANVAS_SIZE_MIN.y)
                                {
                                    _canvas.Colors = new ConsoleColor[height.Value][];
                                    Input.ResetString();
                                    SetStage(Stages.Create_Size_Width);
                                }
                            }
                            break;

                        case Stages.Create_Size_Width:
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

                                    Input.ResetString();
                                    Window.Clear();
                                    SetStage(Stages.Edit);
                                }
                            }
                            break;
                    }
                }, key: ConsoleKey.Enter),
                new Keybind(() =>
                {
                    switch (Stage)
                    {
                        case Stages.Create_Name:
                            {
                                _canvas = null!;
                                Input.ResetString();
                                SetStage(Stages.MainMenu);
                            }
                            break;

                        case Stages.Create_Size_Height:
                            {
                                Input.String = _canvas.Title;
                                SetStage(Stages.Create_Name);
                            }
                            break;

                        case Stages.Create_Size_Width:
                            {
                                Input.String = _canvas.Colors.Length.ToString();
                                SetStage(Stages.Create_Size_Height);
                            }
                            break;
                    }
                }, key: ConsoleKey.Escape));
        }

        protected override void SetStage(Stages stage)
        {
            // Enable cursor if going into Edit mode
            Cursor.Visible = stage == Stages.Edit;
            base.SetStage(stage);
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
