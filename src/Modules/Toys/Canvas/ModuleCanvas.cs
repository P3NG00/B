using B.Inputs;
using B.Utils;
using B.Utils.Enums;
using B.Utils.Extensions;
using B.Utils.Themes;

namespace B.Modules.Toys.Canvas
{
    public sealed class ModuleCanvas : Module<ModuleCanvas.Stages>
    {
        #region Constants

        // The max length a string may be when entered in canvas creation.
        private const int MAX_INPUT_LENGTH = 25;
        // The max amount of canvases to display per page when selecting a canvas.
        private const int MAX_CANVASES_PER_PAGE = 10;
        // Constant height offset when in edit mode.
        private const int CANVAS_EDIT_HEIGHT = 5;

        #endregion



        #region Universal Properties

        // Module Title.
        public static string Title => "Canvas";
        // Path where canvases are serialized.
        public static string DirectoryPath => Program.DataPath + @"canvas\";

        #endregion



        #region Private Variables

        // Constant thickness of the canvas border.
        private static Vector2 CANVAS_BORDER_PAD => new(2, 1);
        // Constant minimum canvas creation size.
        private static Vector2 CANVAS_SIZE_MIN => new(20, 10);

        // List of loaded canvases.
        private List<CanvasInfo> _canvases = new();
        // Canvas currently selected to modify.
        private CanvasInfo _canvas = null!;
        // Current color to paint in.
        private ConsoleColor _color = ConsoleColor.Black;
        // Size of the brush (starting from top-left).
        private Vector2 _brushSize = Vector2.One;
        // Location of the cursor on the canvas
        private Vector2 _cursorPos = Vector2.Zero;
        // Keybinds for creation stages
        private Keybind[] _keybindsCreationStage;
        // Choice for drawing on the canvas
        private Choice _choiceCanvasDrawing;
        // Whether the canvas has been drawn.
        // Used to tell when to re-draw the canvas because
        // large canvases can take significant time to draw.
        private bool _drawn = false;

        #endregion



        #region Constructors

        // Creates new instance of ModuleCanvas.
        public ModuleCanvas() : base(Stages.MainMenu)
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);
            else
            {
                // If directory exists, iterate through each file in it
                foreach (string filePath in Directory.GetFiles(DirectoryPath))
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
                                if (!string.IsNullOrWhiteSpace(Input.String) && !_canvases.Select(c => c.Title).Contains(Input.String))
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

                                if (width.HasValue && width.Value >= CANVAS_SIZE_MIN.x)
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

                                if (height.HasValue && height.Value >= CANVAS_SIZE_MIN.y)
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
                                    _canvas.Save();
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
            // Keybind Help
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() => SetStage(Stages.Edit_Keybinds), "Keybinds", key: ConsoleKey.F1));
            // Save & Quit
            _choiceCanvasDrawing.AddKeybind(Keybind.Create(() =>
            {
                if (!_canvases.Contains(_canvas))
                    _canvases.Add(_canvas);

                _canvas.Save();
                _brushSize = Vector2.One;
                _cursorPos = Vector2.Zero;
                // Reset MouseInputType when leaving edit mode.
                Mouse.InputType = Mouse.MouseInputType.Click;
                SetStage(Stages.List);
            }, "Save & Quit", key: ConsoleKey.Escape));
        }

        #endregion



        #region Override Methods

        // Module Loop.
        public sealed override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        bool hasCanvases = !_canvases.IsEmpty();
                        Window.SetSize(20, hasCanvases ? 8 : 7);
                        Cursor.Set(0, 1);
                        Choice choice = new(Title);
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
                        choice.AddKeybind(Keybind.CreateModuleExit(this));
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
                            maxEntriesPerPage: MAX_CANVASES_PER_PAGE,
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
                            }
                        );
                    }
                    break;

                case Stages.View:
                    {
                        Window.Size = _canvas.Size + (CANVAS_BORDER_PAD * 2);
                        Cursor.Position = CANVAS_BORDER_PAD;
                        _canvas.Draw();
                        Input.WaitFor(ConsoleKey.Escape, true);
                        SetStage(Stages.List);
                    }
                    break;

                case Stages.Create_Name:
                case Stages.Create_Size_Width:
                case Stages.Create_Size_Height:
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
                        Input.RequestLine(MAX_INPUT_LENGTH, _keybindsCreationStage);

                        // Local function
                        void PrintCreationStageInfo(string preface, string value) => Window.Print($"{preface}: {value,-MAX_INPUT_LENGTH}");
                    }
                    break;

                case Stages.Edit:
                    {
                        Vector2 windowSize = _canvas.Size + (CANVAS_BORDER_PAD * 2);
                        windowSize.y += CANVAS_EDIT_HEIGHT + 3;
                        Window.Size = windowSize;
                        // Print top info
                        Cursor.Set(2, 1);
                        Window.Print($"Color: {_color,-11}");
                        Cursor.Set(2, 2);
                        Window.Print($"Position: {_cursorPos,-10}");
                        Cursor.Set(2, 3);
                        Window.Print($"Brush: {_brushSize,-10}");
                        // Print canvas if necessary
                        Vector2 cursorPos;
                        if (!_drawn)
                        {
                            _drawn = true;
                            cursorPos = CANVAS_BORDER_PAD;
                            cursorPos.y += CANVAS_EDIT_HEIGHT;
                            Cursor.Position = cursorPos;
                            _canvas.Draw();
                            // Print border after drawing canvas because it will change the size of the Window
                            Cursor.Set(2, 4);
                            Window.Print('-'.Loop(Window.Width - 4));
                        }
                        // Request input and reposition cursor
                        Cursor.y = (CANVAS_BORDER_PAD.y * 2) + _canvas.Height + CANVAS_EDIT_HEIGHT;
                        var keyInfo = _choiceCanvasDrawing.Request(() =>
                        {
                            // Make cursor big and visible
                            Cursor.Size = 100;
                            Cursor.Visible = true;
                            // Set to brush position
                            Vector2 cursorPos = _cursorPos + CANVAS_BORDER_PAD;
                            cursorPos.y += CANVAS_EDIT_HEIGHT;
                            Cursor.Position = cursorPos;
                        });
                        // If Debug key (F12) was pressed redraw canvas because window will be cleared
                        if (keyInfo.Key == ConsoleKey.F12)
                            _drawn = false;
                        // Check for mouse input
                        if (Mouse.LeftButtonDown)
                        {
                            // Find location of mouse on canvas
                            Vector2 mousePosOnCanvas = Mouse.Position - CANVAS_BORDER_PAD;
                            mousePosOnCanvas.y -= CANVAS_EDIT_HEIGHT;
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

                case Stages.Edit_Keybinds:
                    {
                        Window.SetSize(44, 10);
                        Cursor.y = 1;
                        Choice choice = new("Canvas Keybinds");
                        choice.AddText(new("[Paint] Space / Left Click"));
                        choice.AddText(new("[Move Cursor] W, A, S, D / Arrow Keys"));
                        choice.AddText(new("[Paint & Move] NumPad 2, 4, 6, 8"));
                        choice.AddText(new("[Adjust Brush Size] Control + Arrow Keys"));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Edit), "Back", key: ConsoleKey.Escape));
                        choice.Request();
                    }
                    break;

                case Stages.Edit_ColorSelect:
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

        // Updates the current stage.
        // Updates MouseInputType if going into edit mode.
        // Resets drawn status. Setting stage clears window.
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

        // Paints the current color at the current canvas cursor position.
        private void Paint()
        {
            int xStart = _cursorPos.x;
            int yStart = _cursorPos.y;
            // Account for edge of canvas
            int xEnd = Math.Min(xStart + _brushSize.x, _canvas.Size.x);
            int yEnd = Math.Min(yStart + _brushSize.y, _canvas.Size.y);

            for (int y = yStart; y < yEnd; y++)
            {
                for (int x = xStart; x < xEnd; x++)
                {
                    // Update color on canvas
                    Vector2 canvasPos = new(x, y);
                    _canvas.Color(canvasPos) = _color;
                    // Set position of cursor
                    Vector2 cursorPos = canvasPos + CANVAS_BORDER_PAD;
                    cursorPos.y += CANVAS_EDIT_HEIGHT;
                    Cursor.Position = cursorPos;
                    // Print color
                    Window.Print(' ', new ColorPair(colorBack: _color));
                }
            }

            Cursor.Position = _cursorPos;
        }

        // Paints the current color at the current canvas cursor position,
        // then moves in specified direction to paint the next position.
        private void PaintInDirection(Direction direction)
        {
            // Paint on current position
            Paint();
            // Move cursor
            MoveCursor(direction);
            // Paint on new position
            Paint();
        }

        // Modifies the size of the brush.
        private void ResizeBrush(Direction direction) => MoveVec(ref _brushSize, direction);

        // Modifies the position of the canvas cursor.
        private void MoveCursor(Direction direction) => MoveVec(ref _cursorPos, direction);

        // Modifies given Vector2 by direction.
        // Flips Up/Down direction due to the
        // direction that the canvas is printed.
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

        // Module Stages.
        public enum Stages
        {
            // Create a new canvas or do something with an existing one.
            MainMenu,
            // View, edit, or delete existing canvases.
            List,
            // View an existing canvas.
            View,
            // Stages for creating new canvases.
            Create_Name,
            Create_Size_Width,
            Create_Size_Height,
            // Editing canvases.
            Edit,
            // View descriptions of keybinds for Edit mode.
            Edit_Keybinds,
            // Select a color to use in Edit mode.
            Edit_ColorSelect,
        }

        #endregion
    }
}
