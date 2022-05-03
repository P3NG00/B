using B.Inputs;
using B.Utils;
using B.Utils.Enums;
using B.Utils.Extensions;
using B.Utils.Themes;

namespace B.Options.Toys.Canvas
{
    public sealed class OptionCanvas : Option<OptionCanvas.Stages>
    {
        #region TODOs

        // TODO add image importing to canvas grid

        #endregion


        #region Constants

        private const int MAX_INPUT_LENGTH = 25;
        private const int MAX_CANVASES_PER_PAGE = 10;

        #endregion



        #region Universal Properties

        public static string Title => "Canvas";
        public static Vector2 CANVAS_BORDER_PAD => new(2, 1);

        public static string DirectoryPath => Program.DataPath + @"canvas\";

        #endregion



        #region Private Variables

        private static Vector2 CANVAS_SIZE_MIN => new(20, 10);
        private static Vector2 CURSOR_POS_MIN => new(0, 4);

        private bool _drawn = false;
        private List<CanvasInfo> _canvases = new();
        private CanvasInfo _canvas = null!;
        private ConsoleColor _color = ConsoleColor.Black;
        // The size of the brush
        private Vector2 _brushSize = Vector2.One;
        // The location of the cursor on the canvas
        private Vector2 _cursorPos = Vector2.Zero;

        private Keybind[] _keybindsCreationStage;
        private Choice _choiceCanvasDrawing;

        #endregion



        #region Constructors

        public OptionCanvas() : base(Stages.MainMenu)
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(OptionCanvas.DirectoryPath))
                Directory.CreateDirectory(OptionCanvas.DirectoryPath);
            else
            {
                // If directory exists, iterate through each file in it
                foreach (string filePath in Directory.GetFiles(OptionCanvas.DirectoryPath))
                {
                    // Deserialize file as CanvasInfo
                    CanvasInfo info = Data.Deserialize<CanvasInfo>(filePath);
                    // Set canvas info's title to the file name
                    info.Title = Path.GetFileNameWithoutExtension(filePath);
                    // Add to canvas list
                    _canvases.Add(info);
                }
            }

            _keybindsCreationStage = new Keybind[] {
                // ConsoleKey.Enter
                Keybind.Create(() =>
                {
                    switch (Stage)
                    {
                        case Stages.Create_Name:
                            {
                                if (!string.IsNullOrWhiteSpace(Input.String) && !_canvases.FromEach(c => c.Title).Contains(Input.String))
                                {
                                    _canvas.Title = Input.String.Trim();
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
                                    _canvas.Colors = new ConsoleColor[width.Value][];
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
                                    int width = _canvas.Colors.Length;
                                    _canvas.Colors = new ConsoleColor[height.Value][];

                                    for (int i = 0; i < _canvas.Colors.Length; i++)
                                    {
                                        ConsoleColor[] row = new ConsoleColor[width];
                                        Array.Fill(row, ConsoleColor.White);
                                        _canvas.Colors[i] = row;
                                    }

                                    Input.ResetString();
                                    SetStage(Stages.Edit);
                                }
                            }
                            break;
                    }
                }, key: ConsoleKey.Enter),
                // ConsoleKey.Escape
                Keybind.Create(() =>
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

                        case Stages.Create_Size_Width:
                            {
                                Input.String = _canvas.Title;
                                Cursor.Set(2, 5);
                                Window.Print(' '.Loop(33));
                                SetStage(Stages.Create_Name);
                            }
                            break;

                        case Stages.Create_Size_Height:
                            {
                                Input.String = _canvas.Colors.Length.ToString();
                                Cursor.Set(2, 6);
                                Window.Print(' '.Loop(33));
                                SetStage(Stages.Create_Size_Width);
                            }
                            break;
                    }
                }, key: ConsoleKey.Escape)};

            _choiceCanvasDrawing = new();
            // TODO print things like "use wasd for ..." and "arrow keys for "
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => MoveCursor(Direction.Up), keyChar: 'w', key: ConsoleKey.UpArrow));
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => MoveCursor(Direction.Down), keyChar: 's', key: ConsoleKey.DownArrow));
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => MoveCursor(Direction.Left), keyChar: 'a', key: ConsoleKey.LeftArrow));
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => MoveCursor(Direction.Right), keyChar: 'd', key: ConsoleKey.RightArrow));
            // Paint in Direction
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => PaintInDirection(Direction.Up), keyChar: '8'));
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => PaintInDirection(Direction.Down), keyChar: '2'));
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => PaintInDirection(Direction.Left), keyChar: '4'));
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => PaintInDirection(Direction.Right), keyChar: '6'));
            // Paint
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(Paint, key: ConsoleKey.Spacebar));
            // Resize Brush 
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => ResizeBrush(Direction.Up), key: ConsoleKey.UpArrow, modifiers: ConsoleModifiers.Control));
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => ResizeBrush(Direction.Down), key: ConsoleKey.DownArrow, modifiers: ConsoleModifiers.Control));
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => ResizeBrush(Direction.Left), key: ConsoleKey.LeftArrow, modifiers: ConsoleModifiers.Control));
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => ResizeBrush(Direction.Right), key: ConsoleKey.RightArrow, modifiers: ConsoleModifiers.Control));
            // Color Select
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() =>
            {
                SetStage(Stages.ColorSelect);
            }, key: ConsoleKey.F1));
            // Exit
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() =>
            {
                if (!_canvases.Contains(_canvas))
                    _canvases.Add(_canvas);

                Save();
                _brushSize = Vector2.One;
                _cursorPos = Vector2.Zero;
                // Reset MouseInputType when leaving edit mode.
                Mouse.InputType = Mouse.MouseInputType.Click;
                SetStage(Stages.List);
            }, key: ConsoleKey.Escape));
        }

        #endregion



        #region Override Methods

        public sealed override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        bool hasCanvases = !_canvases.IsEmpty();
                        Window.SetSize(20, hasCanvases ? 8 : 7);
                        Cursor.Set(0, 1);
                        Choice choice = new(OptionCanvas.Title);
                        choice.AddKeybind(Keybind.Create(() =>
                        {
                            _canvas = new();
                            Input.ResetString();
                            SetStage(Stages.Create_Name);
                        }, "Create", '1'));

                        if (hasCanvases)
                        {
                            choice.AddKeybind(Keybind.Create(() =>
                            {
                                Input.ScrollIndex = 0;
                                SetStage(Stages.List);
                            }, "List", '2'));
                        }

                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateOptionExit(this));
                        choice.Request();
                    }
                    break;

                case Stages.List:
                    {
                        Window.SetSize(32, _canvases.Count + 12);
                        _canvas = _canvases[Input.ScrollIndex];
                        Cursor.y = 1;
                        Input.RequestScroll(
                            items: _canvases,
                            getText: canvas => canvas.Title,
                            title: "Canvases",
                            maxEntriesPerPage: OptionCanvas.MAX_CANVASES_PER_PAGE,
                            exitKeybind: Keybind.Create(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape),
                            extraKeybinds: new Keybind[] {
                                Keybind.Create(() =>
                                {
                                    Input.ScrollIndex = 0;
                                    SetStage(Stages.View);
                                }, "View", key: ConsoleKey.Enter),
                                Keybind.Create(() =>
                                {
                                    Input.ScrollIndex = 0;
                                    _brushSize = Vector2.One;
                                    _cursorPos = Vector2.Zero;
                                    SetStage(Stages.Edit);
                                }, "Edit", key: ConsoleKey.Tab),
                                Keybind.CreateConfirmation(() =>
                                {
                                    Input.ScrollIndex = 0;
                                    File.Delete(_canvas.FilePath);
                                    _canvases.Remove(_canvas);

                                    if (_canvases.IsEmpty())
                                        SetStage(Stages.MainMenu);
                                }, $"Delete {_canvas.Title}?", "Delete", key: ConsoleKey.Delete),
                            });
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

                case Stages.Create_Size_Width: ShowCreationStage(); break;

                case Stages.Create_Size_Height: ShowCreationStage(); break;

                case Stages.Edit:
                    {
                        // TODO check if mouse is held and paint where it is hovering

                        if (!_drawn)
                        {
                            _drawn = true;
                            _canvas.Draw(OptionCanvas.CURSOR_POS_MIN);
                        }

                        // Print top info
                        Cursor.Set(2, 0);
                        Window.Print($"Color: {_color,-11}");
                        Cursor.Set(2, 1);
                        Window.Print($"Position: {_cursorPos,-10}");
                        Cursor.Set(2, 2);
                        Window.Print($"Brush: {_brushSize,-10}");
                        // Print border
                        Cursor.Set(2, 3);
                        Window.Print('-'.Loop(Window.Width - 4));
                        // Request input and reposition cursor
                        _choiceCanvasDrawing.Request(() =>
                        {
                            // Make cursor big and visible
                            Cursor.Size = 100;
                            Cursor.Visible = true;
                            // Set to brush position
                            Cursor.Position = _cursorPos + OptionCanvas.CURSOR_POS_MIN + OptionCanvas.CANVAS_BORDER_PAD;
                        });
                        // Find location of mouse on canvas
                        Vector2 mousePosOnCanvas = Mouse.Position - OptionCanvas.CURSOR_POS_MIN - OptionCanvas.CANVAS_BORDER_PAD;
                        // Check for mouse input
                        if (Mouse.LeftButtonDown)
                        {
                            // Check if mouse is in canvas
                            bool inX = mousePosOnCanvas.x >= 0 && mousePosOnCanvas.x < _canvas.Width;
                            bool inY = mousePosOnCanvas.y >= 0 && mousePosOnCanvas.y < _canvas.Height;
                            if (inX && inY)
                            {
                                // Move cursor
                                _cursorPos = mousePosOnCanvas;
                                // Paint
                                Paint();
                            }
                        }
                        // Update cursor settings
                        Program.Settings.UpdateCursor();
                        // Fix cursor position
                        _cursorPos = _cursorPos.Clamp(Vector2.Zero, _canvas.Size - Vector2.One);
                        // Fix brush size
                        _brushSize = _brushSize.Clamp(Vector2.One, _canvas.Size);
                    }
                    break;

                case Stages.ColorSelect:
                    {
                        Window.SetSize(32, 26);
                        ConsoleColor[] colors = Util.ConsoleColors;
                        Cursor.y = 1;
                        Input.RequestScroll(
                            items: colors,
                            getText: c => $" {c.ToString(),-12}",
                            getTextColor: (c, i) => c == ConsoleColor.Black || c.ToString().StartsWith("Dark") ? ConsoleColor.White : ConsoleColor.Black,
                            getBackgroundColor: (c, i) => c,
                            title: "Color Select",
                            exitKeybind: Keybind.Create(ExitColorSelect, "Back", key: ConsoleKey.Escape),
                            extraKeybinds: Keybind.Create(() =>
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

        public sealed override void Save() => Data.Serialize(_canvas.FilePath, _canvas);

        protected sealed override void SetStage(Stages stage)
        {
            // Set MouseInputType if editing a canvas
            if (stage == Stages.Edit)
                Mouse.InputType = Mouse.MouseInputType.Hold;

            _drawn = false;
            base.SetStage(stage);
        }

        #endregion



        #region Private Methods

        private void ShowCreationStage()
        {
            Window.SetSize(35, 8);
            Cursor.Set(2, 1);
            Window.Print("New Canvas");
            Cursor.Set(2, 3);
            // Print canvas name or current input
            PrintCreationStageInfo("Name", Stage == Stages.Create_Name ? Input.String : _canvas.Title);

            if (Stage != Stages.Create_Name)
            {
                Cursor.Set(2, 5);
                string print = string.Empty;

                if (Stage == Stages.Create_Size_Width)
                    print = Input.String;
                else if (Stage == Stages.Create_Size_Height)
                    print = _canvas.Colors.Length.ToString();

                PrintCreationStageInfo("Width", print);
            }

            if (Stage == Stages.Create_Size_Height)
            {
                Cursor.Set(2, 6);
                PrintCreationStageInfo("Height", Input.String);
            }

            // Request line input
            Input.RequestLine(OptionCanvas.MAX_INPUT_LENGTH, _keybindsCreationStage);

            // Local function
            void PrintCreationStageInfo(string preface, string value) => Window.Print($"{preface}: {value,-OptionCanvas.MAX_INPUT_LENGTH}");
        }

        private void Paint()
        {
            // TODO account for canvas edges
            int endX = Math.Min(_cursorPos.x + _brushSize.x, _canvas.Size.x);
            int endY = Math.Min(_cursorPos.y + _brushSize.y, _canvas.Size.y);

            for (int y = _cursorPos.y; y < endY; y++)
            {
                for (int x = _cursorPos.x; x < endX; x++)
                {
                    Vector2 pos = new(x, y);
                    _canvas.Color(pos) = _color;
                    Cursor.Position = pos + OptionCanvas.CURSOR_POS_MIN + OptionCanvas.CANVAS_BORDER_PAD;
                    Window.Print(' ', new ColorPair(colorBack: _color));
                }
            }

            Cursor.Position = _cursorPos;
        }

        private void PaintInDirection(Direction direction)
        {
            // Paint on current position
            Paint();
            // Move cursor
            MoveCursor(direction);
            // Paint on new position
            Paint();
        }

        private void ResizeBrush(Direction direction) => MoveVec(ref _brushSize, direction);

        private void MoveCursor(Direction direction) => MoveVec(ref _cursorPos, direction);

        private void MoveVec(ref Vector2 vec, Direction direction)
        {
            Vector2 dirVec;

            switch (direction)
            {
                // Since coordinates start from top-left corner, up and down are flipped to move appropriately.
                case Direction.Up: dirVec = Vector2.Down; break; // Move up (y - 1 or Vector2.Down)
                case Direction.Down: dirVec = Vector2.Up; break; // Move down (y + 1 or Vector2.Up)
                default: dirVec = direction; break;
            }

            vec.Move(dirVec);
        }

        #endregion



        #region Enums

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
            View,
            Create_Name,
            Create_Size_Width,
            Create_Size_Height,
            Edit,
            ColorSelect,
        }

        #endregion
    }
}
