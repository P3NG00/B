using B.Inputs;
using B.Utils;
using B.Utils.Enums;
using B.Utils.Extensions;

namespace B.Options.Games.Adventure
{
    public sealed class OptionAdventure : Option<OptionAdventure.Stages>
    {
        #region TODOs

        // TODO implement some color printing (examples: tiles, player, coins, doors, border, text, etc.)
        // TODO add option in adventure to enable/disable color printing

        #endregion



        #region Constants

        public const string CHAR_EMPTY = "  ";
        public const string CHAR_PLAYER = "()";
        public const string CHAR_DOOR = "[]";
        public const string CHAR_COIN = "<>";
        public const string CHAR_WALL = "▓▓";
        public const string CHAR_INTERACTABLE = "░░";
        public const string CHAR_BORDER_HORIZONTAL = "==";
        public const string CHAR_BORDER_VERTICAL = "||";
        public const string CHAR_CORNER_A = "//";
        public const string CHAR_CORNER_B = @"\\";
        public const string MESSAGE_EMPTY = "...";

        #endregion



        #region Public Properties

        public static string Title => "Adventure!";
        public static string DirectoryPath => Program.DataPath + @"adventure\";
        public static string FilePath => DirectoryPath + "save";
        public static Grid CurrentGrid => OptionAdventure._grids[OptionAdventure.Info.GridID];

        #endregion



        #region Universal Variables

        public static string Message = OptionAdventure.MESSAGE_EMPTY;
        public static AdventureInfo Info = new();

        #endregion



        #region Private Variables

        private static Grid[] _grids = new Grid[0];
        private Choice _choiceGame;

        #endregion



        #region Private Properties

        private Vector2 PlayerPosition
        {
            get => OptionAdventure.Info.Position;
            set => OptionAdventure.Info.Position = value;
        }

        #endregion



        #region Constructors

        static OptionAdventure() => OptionAdventure.InitializeGrids();

        public OptionAdventure() : base(Stages.MainMenu)
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
            _choiceGame.AddKeybind(Keybind.Create(() => OptionAdventure.Info.Speed++, key: ConsoleKey.Add));
            _choiceGame.AddKeybind(Keybind.Create(() => OptionAdventure.Info.Speed--, key: ConsoleKey.Subtract));
            _choiceGame.AddSpacer();
            _choiceGame.AddKeybind(Keybind.Create(() =>
            {
                Save();
                SetStage(Stages.MainMenu);
            }, "Quit", key: ConsoleKey.Escape));
        }

        #endregion



        #region Override Methods

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        bool fileExists = File.Exists(OptionAdventure.FilePath);
                        Window.Clear();
                        Window.SetSize(20, fileExists ? 8 : 7);
                        Cursor.Set(0, 1);
                        Choice choice = new(OptionAdventure.Title);

                        if (fileExists)
                        {
                            choice.AddKeybind(Keybind.CreateConfirmation(() => InitGame(true), "Are you sure you want to override the current game?", "New Game", '1'));
                            choice.AddKeybind(Keybind.Create(() => InitGame(false), "Continue", '2'));
                        }
                        else
                            choice.AddKeybind(Keybind.Create(() => InitGame(true), "New Game", '1'));

                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateOptionExit(this));
                        choice.Request();

                        // Local Method
                        void InitGame(bool newGame)
                        {
                            if (newGame)
                            {
                                OptionAdventure.Info = new();
                                Grid currentGrid = OptionAdventure.CurrentGrid;
                                PlayerPosition = new(currentGrid.Width / 2, currentGrid.Height / 2);
                            }
                            else
                                OptionAdventure.Info = Data.Deserialize<AdventureInfo>(OptionAdventure.FilePath);

                            Window.Clear();
                            SetStage(Stages.Game);
                        }
                    }
                    break;

                case Stages.Game:
                    {
                        // TODO use different border styles

                        Grid currentGrid = OptionAdventure.CurrentGrid;
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
                            Cursor.y++;
                        }

                        Window.SetSize(currentGrid.RealWidth + 8, consoleHeight);
                        string borderHorizontal = OptionAdventure.CHAR_BORDER_HORIZONTAL.Loop(currentGrid.Width);
                        Cursor.y++;
                        Cursor.x = 2;
                        Window.Print($"{OptionAdventure.CHAR_CORNER_A}{borderHorizontal}{OptionAdventure.CHAR_CORNER_B}");

                        for (int y = currentGrid.Height - 1; y >= 0; y--)
                        {
                            Cursor.y++;
                            string s = string.Empty;

                            for (int x = 0; x < currentGrid.Width; x++)
                            {
                                Vector2 pos = new(x, y);

                                if (pos == PlayerPosition)
                                    s += OptionAdventure.CHAR_PLAYER;
                                else if (currentGrid.HasCoinAt(pos))
                                    s += OptionAdventure.CHAR_COIN;
                                else
                                    s += currentGrid.GetTile(pos).Chars;
                            }

                            Cursor.x = 2;
                            Window.Print(OptionAdventure.CHAR_BORDER_VERTICAL + s + OptionAdventure.CHAR_BORDER_VERTICAL);
                        }

                        Cursor.y++;
                        Cursor.x = 2;
                        Window.Print(OptionAdventure.CHAR_CORNER_B + borderHorizontal + OptionAdventure.CHAR_CORNER_A);
                        Cursor.y += 2;
                        Cursor.x = 3;
                        Window.Print($"> {OptionAdventure.Message}");
                        Cursor.y += 2;
                        Cursor.x = 0;
                        OptionAdventure.Message = string.Format("{0,-" + (currentGrid.RealWidth - 7) + "}", OptionAdventure.MESSAGE_EMPTY);
                        string format = "{0,9}: {1,-5}";
                        Window.Print(string.Format(format, "Coins", OptionAdventure.Info.Coins));
                        Cursor.y++;
                        Cursor.x = 0;
                        Window.Print(string.Format(format, "Speed", OptionAdventure.Info.Speed));
                        Cursor.y += 2;
                        Cursor.x = 6;
                        Window.Print("Move) W A S D");
                        Cursor.y++;
                        Cursor.x = 2;
                        Window.Print("Interact) Space");
                        Cursor.y++;
                        Cursor.x = 5;
                        Window.Print("Speed) + -");
                        Cursor.y++;
                        _choiceGame.Request();
                    }
                    break;
            }
        }

        public override void Save() => Data.Serialize(OptionAdventure.FilePath, OptionAdventure.Info);

        #endregion



        #region Private Methods

        private void MovePlayer(Direction direction)
        {
            bool stop = false;

            for (int i = 0; i < OptionAdventure.Info.Speed && !stop; i++)
            {
                Vector2 newPos = PlayerPosition + direction;
                Grid currentGrid = OptionAdventure.CurrentGrid;

                if (newPos.x >= 0 && newPos.x < currentGrid.Width && newPos.y >= 0 && newPos.y < currentGrid.Height)
                {
                    Tile tile = currentGrid.GetTile(newPos);
                    currentGrid.MoveTo(newPos);
                    Tile.TileTypes tileType = tile.TileType;
                    stop = tileType.StopsMovement() || tileType == Tile.TileTypes.Door;

                    if (!stop)
                        PlayerPosition = newPos;
                }
            }
        }

        private void Interact(Direction direction) => OptionAdventure.CurrentGrid.Interact(PlayerPosition + direction);

        #endregion



        #region Universal Methods

        public static void InitializeGrids()
        {
            // ' ' | EMPTY
            // 'c' | COIN
            // 'd' | DOOR
            // 'w' | WALL
            // 'i' | TILE_INTERACTABLE

            // Initialize Grid Array
            OptionAdventure._grids = new Grid[3];

            // Grid 0
            string[] sa = Grid.CreateGrid(new(15));
            sa[13] = " wwwwwwwwwwwww ";
            sa[12] = "  w         w  ";
            sa[11] = "       i       ";
            sa[7] = "   w       w  d";
            sa[3] = "   w   c   w   ";
            sa[1] = " wwwwwwwwwwwww ";
            OptionAdventure._grids[0] = new(sa);
            OptionAdventure._grids[0].AddInteraction(new(7, 11), () => OptionAdventure.Message = "You touched it!");

            // Grid 1
            sa = Grid.CreateGrid(new(17, 21));
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
            OptionAdventure._grids[1] = new(sa);

            // Grid 2
            sa = Grid.CreateGrid(new(13, 9));
            sa[4] = "           i ";
            OptionAdventure._grids[2] = new(sa);
            OptionAdventure._grids[2].AddInteraction(new(11, 4), () => OptionAdventure.Message = "The End...?");

            // Add Doors after initializing each room
            OptionAdventure._grids[0].AddDoor(new(14, 7), new(1, new(8)));
            OptionAdventure._grids[1].AddDoor(new(8, 13), new(2, new(1, 4)));

            // Seal Grids (meant for checking if all doors and interactions are implemented)
            foreach (Grid grid in OptionAdventure._grids)
                grid.Seal();
        }

        #endregion



        #region Enums

        public enum Stages
        {
            MainMenu,
            Game,
        }

        #endregion
    }
}
