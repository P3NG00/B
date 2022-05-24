using B.Inputs;
using B.Utils;
using B.Utils.Enums;
using B.Utils.Extensions;

namespace B.Modules.Games.Adventure
{
    public sealed class ModuleAdventure : Module<ModuleAdventure.Stages>
    {
        #region Constants

        // Empty Tile.
        public const string CHAR_EMPTY = "  ";
        // Player.
        public const string CHAR_PLAYER = "()";
        // Door Tile.
        public const string CHAR_DOOR = "[]";
        // Coin Tile.
        public const string CHAR_COIN = "<>";
        // Wall Tile.
        public const string CHAR_WALL = "▓▓";
        // Interactable Tile.
        public const string CHAR_INTERACTABLE = "░░";
        // Border Tiles.
        public const string CHAR_BORDER_HORIZONTAL = "==";
        public const string CHAR_BORDER_VERTICAL = "||";
        public const string CHAR_BORDER_CORNER_A = "//";
        public const string CHAR_BORDER_CORNER_B = @"\\";
        // Empty Message.
        public const string MESSAGE_EMPTY = "...";

        #endregion



        #region Public Properties

        // Module Title.
        public static string Title => "Adventure!";
        // Relative path to Adventure's game data directory.
        public static string DirectoryPath => Program.DataPath + @"adventure\";
        // Relative path to Adventure's save game file.
        public static string FilePath => DirectoryPath + "save";
        // The grid the player is currently in.
        public static Grid CurrentGrid => _grids[Info.GridID];

        #endregion



        #region Universal Variables

        // Message to display to player.
        public static string Message = MESSAGE_EMPTY;
        // Game Data.
        public static AdventureInfo Info = new();

        #endregion



        #region Private Variables

        // Array of loaded grids.
        // Indexed by grid ID.
        private static Grid[] _grids = new Grid[0];
        // Cached choice for game input.
        private Choice _choiceGame;

        #endregion



        #region Private Properties

        // The player's position on the grid.
        private Vector2 PlayerPosition
        {
            get => Info.Position;
            set => Info.Position = value;
        }

        #endregion



        #region Constructors

        // Initializes Grids.
        static ModuleAdventure() => InitializeGrids();

        // Creates a new instance of ModuleAdventure.
        public ModuleAdventure() : base(Stages.MainMenu)
        {
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            _choiceGame = new();
            _choiceGame.AddKeybind(Keybind.Create(() => MovePlayer(Direction.Up), keyChar: 'w', key: ConsoleKey.NumPad8));
            _choiceGame.AddKeybind(Keybind.Create(() => MovePlayer(Direction.Left), keyChar: 'a', key: ConsoleKey.NumPad4));
            _choiceGame.AddKeybind(Keybind.Create(() => MovePlayer(Direction.Down), keyChar: 's', key: ConsoleKey.NumPad2));
            _choiceGame.AddKeybind(Keybind.Create(() => MovePlayer(Direction.Right), keyChar: 'd', key: ConsoleKey.NumPad6));
            _choiceGame.AddKeybind(Keybind.Create(() => Interact(Direction.Up), key: ConsoleKey.UpArrow));
            _choiceGame.AddKeybind(Keybind.Create(() => Interact(Direction.Left), key: ConsoleKey.LeftArrow));
            _choiceGame.AddKeybind(Keybind.Create(() => Interact(Direction.Down), key: ConsoleKey.DownArrow));
            _choiceGame.AddKeybind(Keybind.Create(() => Interact(Direction.Right), key: ConsoleKey.RightArrow));
            _choiceGame.AddKeybind(Keybind.Create(() => Info.Speed++, key: ConsoleKey.Add));
            _choiceGame.AddKeybind(Keybind.Create(() => Info.Speed--, key: ConsoleKey.Subtract));
            _choiceGame.AddSpacer();
            _choiceGame.AddKeybind(Keybind.Create(() =>
            {
                // Save game data
                Data.Serialize(FilePath, Info);
                // Go to main menu
                SetStage(Stages.MainMenu);
            }, "Quit", key: ConsoleKey.Escape));
        }

        #endregion



        #region Override Methods

        // Module Loop.
        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        bool fileExists = File.Exists(FilePath);
                        Window.SetSize(20, fileExists ? 8 : 7);
                        Cursor.Set(0, 1);
                        Choice choice = new(Title);

                        if (fileExists)
                        {
                            choice.AddKeybind(Keybind.CreateConfirmation(() => InitGame(true), "Are you sure you want to override the current game?", "New Game", '1'));
                            choice.AddKeybind(Keybind.Create(() => InitGame(false), "Continue", '2'));
                        }
                        else
                            choice.AddKeybind(Keybind.Create(() => InitGame(true), "New Game", '1'));

                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateModuleExit(this));
                        choice.Request();

                        // Local Method
                        void InitGame(bool newGame)
                        {
                            if (newGame)
                            {
                                Info = new();
                                Grid currentGrid = CurrentGrid;
                                PlayerPosition = new(currentGrid.Width / 2, currentGrid.Height / 2);
                            }
                            else
                                Info = Data.Deserialize<AdventureInfo>(FilePath);

                            SetStage(Stages.Game);
                        }
                    }
                    break;

                case Stages.Game:
                    {
                        Grid currentGrid = CurrentGrid;
                        int consoleHeight = currentGrid.Height + 15;
                        Cursor.Reset();

                        if (Program.Settings.DebugMode)
                        {
                            // Extra spaces are added to the end to clear leftover text
                            Cursor.Set(2, 1);
                            Window.Print($"{currentGrid,-7}");
                            Cursor.Set(2, 2);
                            Window.Print($"Pos: {PlayerPosition,-8}");
                            consoleHeight += 3;
                            Cursor.NextLine();
                        }

                        Window.SetSize(currentGrid.RealWidth + 8, consoleHeight);
                        string borderHorizontal = CHAR_BORDER_HORIZONTAL.Loop(currentGrid.Width);
                        Cursor.NextLine(2);
                        Window.Print($"{CHAR_BORDER_CORNER_A}{borderHorizontal}{CHAR_BORDER_CORNER_B}");

                        for (int y = currentGrid.Height - 1; y >= 0; y--)
                        {
                            string s = string.Empty;

                            for (int x = 0; x < currentGrid.Width; x++)
                            {
                                Vector2 pos = new(x, y);

                                if (pos == PlayerPosition)
                                    s += CHAR_PLAYER;
                                else if (currentGrid.HasCoinAt(pos))
                                    s += CHAR_COIN;
                                else
                                    s += currentGrid.GetTile(pos).Chars;
                            }

                            Cursor.NextLine(2);
                            Window.Print(CHAR_BORDER_VERTICAL + s + CHAR_BORDER_VERTICAL);
                        }

                        Cursor.NextLine(2);
                        Window.Print(CHAR_BORDER_CORNER_B + borderHorizontal + CHAR_BORDER_CORNER_A);
                        Cursor.NextLine(3, 2);
                        Window.Print($"> {Message}");
                        Message = string.Format("{0,-" + (currentGrid.RealWidth - 7) + "}", MESSAGE_EMPTY);
                        Cursor.NextLine(lines: 2);
                        string format = "{0,9}: {1,-5}";
                        Window.Print(string.Format(format, "Coins", Info.Coins));
                        Cursor.NextLine();
                        Window.Print(string.Format(format, "Speed", Info.Speed));
                        Cursor.NextLine(6, 2);
                        Window.Print("Move) W A S D");
                        Cursor.NextLine(2);
                        Window.Print("Interact) Space");
                        Cursor.NextLine(5);
                        Window.Print("Speed) + -");
                        Cursor.NextLine();
                        _choiceGame.Request();
                    }
                    break;
            }
        }

        #endregion



        #region Private Methods

        // Move player in specified direction.
        private void MovePlayer(Direction direction)
        {
            bool stop = false;

            for (int i = 0; i < Info.Speed && !stop; i++)
            {
                Vector2 newPos = PlayerPosition + direction;
                Grid currentGrid = CurrentGrid;

                if (newPos.x >= 0 && newPos.x < currentGrid.Width && newPos.y >= 0 && newPos.y < currentGrid.Height)
                {
                    Tile tile = currentGrid.GetTile(newPos);
                    currentGrid.CheckNewPlayerPosition(newPos);
                    Tile.TileTypes tileType = tile.TileType;
                    stop = tileType.StopsMovement() || tileType == Tile.TileTypes.Door;

                    if (!stop)
                        PlayerPosition = newPos;
                }
            }
        }

        // Interact with tile in specified direction.
        private void Interact(Direction direction) => CurrentGrid.Interact(PlayerPosition + direction);

        #endregion



        #region Universal Methods

        // Creates Grids.
        public static void InitializeGrids()
        {
            // Init check
            if (_grids.Length > 0)
                throw new Exception("Grids have already been initialized!");

            // ' ' | EMPTY
            // 'c' | COIN
            // 'd' | DOOR
            // 'w' | WALL
            // 'i' | TILE_INTERACTABLE

            // Initialize Grid Array
            _grids = new Grid[3];

            // Grid 0
            string[] sa = Grid.CreateGrid(15, 15);
            sa[13] = " wwwwwwwwwwwww ";
            sa[12] = "  w         w  ";
            sa[11] = "       i       ";
            sa[7] = "   w       w  d";
            sa[3] = "   w   c   w   ";
            sa[1] = " wwwwwwwwwwwww ";
            _grids[0] = new(sa); // After 'sa' has been used to construct a Grid, it can be re-used.
            _grids[0].AddInteraction(new(7, 11), () => Message = "You touched it!");

            // Grid 1
            sa = Grid.CreateGrid(17, 21);
            sa[13] = "        d        ";
            sa[12] = " www         www ";
            sa[11] = " w             w ";
            sa[10] = " w     w w     w ";
            sa[9] = " w    w   w    w ";
            sa[8] = " w             w ";
            sa[7] = " w    w   w    w ";
            sa[6] = " w     w w     w ";
            sa[5] = " w             w ";
            sa[4] = " www         www ";
            _grids[1] = new(sa);

            // Grid 2
            sa = Grid.CreateGrid(13, 9);
            sa[4] = "           i ";
            _grids[2] = new(sa);
            _grids[2].AddInteraction(new(11, 4), () => Message = "The End...?");

            // Add Doors after initializing each room
            _grids[0].AddDoor(new(14, 7), new(1, new(8)));
            _grids[1].AddDoor(new(8, 13), new(2, new(1, 4)));

            // Seal Grids (meant for checking if all doors and interactions are implemented)
            _grids.ForEach(grid => grid.Seal());
        }

        #endregion



        #region Enums

        // Module Stages.
        public enum Stages
        {
            // Main Menu.
            MainMenu,
            // Adventure.
            Game,
        }

        #endregion
    }
}
