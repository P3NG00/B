using B.Inputs;
using B.Utils;

namespace B.Options.Adventure
{
    public sealed class OptionAdventure : Option
    {
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

        public static string Message = OptionAdventure.MESSAGE_EMPTY;
        public static AdventureInfo Info;

        public static Grid CurrentGrid { get { return OptionAdventure._grids[OptionAdventure.Info.GridID]; } }
        private static Grid[] _grids;

        private readonly string _filePath = Program.DirectoryPath + "adventureInfo";
        private Stage _stage = Stage.MainMenu;

        static OptionAdventure()
        {
            OptionAdventure.InitializeGrids();
        }

        public sealed override void Loop()
        {
            switch (this._stage)
            {
                case Stage.MainMenu:
                    {
                        Console.Clear();
                        int consoleHeight = 7;
                        bool fileExists = File.Exists(this._filePath);
                        if (fileExists) consoleHeight++;
                        Util.SetConsoleSize(20, consoleHeight);
                        Input.Option iob = new Input.Option("Adventure")
                            .AddKeybind(new Keybind(() => this.InitGame(true), "New Game", '1'));

                        if (fileExists)
                            iob.AddKeybind(new Keybind(() => this.InitGame(false), "Continue", '2'));

                        iob.AddSpacer()
                            .AddKeybind(new Keybind(() => this.Quit(), "Back", key: ConsoleKey.Escape))
                            .Request();
                    }
                    break;

                case Stage.Game:
                    {
                        Console.SetCursorPosition(0, 0);
                        Grid currentGrid = OptionAdventure.CurrentGrid;
                        int consoleHeight = currentGrid.Height + 15;

                        if (Program.DebugMode)
                        {
                            // Extra spaces are added to the end to clear leftover text
                            Util.Print(string.Format("{0,-7}", currentGrid), 1, linesBefore: 1);
                            Util.Print(string.Format("Pos: {0,-8}", OptionAdventure.Info.Position), 1);
                            consoleHeight += 3;
                        }

                        Util.SetConsoleSize(currentGrid.RealWidth + 8, consoleHeight);
                        string borderHorizontal = Util.StringOf(OptionAdventure.CHAR_BORDER_HORIZONTAL, currentGrid.Width);
                        Util.Print(string.Format("{0}{1}{2}", OptionAdventure.CHAR_CORNER_A, borderHorizontal, OptionAdventure.CHAR_CORNER_B), 2, linesBefore: 1);
                        Vector2 pos;
                        string s;

                        for (int y = currentGrid.Height - 1; y >= 0; y--)
                        {
                            s = CHAR_BORDER_VERTICAL;

                            for (int x = 0; x < currentGrid.Width; x++)
                            {
                                pos = new Vector2(x, y);

                                if (pos == OptionAdventure.Info.Position)
                                    s += OptionAdventure.CHAR_PLAYER;
                                else if (currentGrid.HasCoinAt(pos))
                                    s += OptionAdventure.CHAR_COIN;
                                else
                                    s += currentGrid.GetTile(pos).Chars;
                            }

                            Util.Print(s + OptionAdventure.CHAR_BORDER_VERTICAL, 2);
                        }

                        Util.Print(string.Format("{0}{1}{2}", OptionAdventure.CHAR_CORNER_B, borderHorizontal, OptionAdventure.CHAR_CORNER_A), 2);
                        Util.Print(string.Format("> {0}", OptionAdventure.Message), 3, linesBefore: 1);
                        OptionAdventure.Message = string.Format("{0,-" + (currentGrid.RealWidth - 7) + "}", OptionAdventure.MESSAGE_EMPTY);
                        string format = "{0,9}: {1,-5}";
                        Util.Print(string.Format(format, "Coins", OptionAdventure.Info.Coins), linesBefore: 1);
                        Util.Print(string.Format(format, "Speed", OptionAdventure.Info.Speed));
                        Util.Print("Move) W A S D", 6, linesBefore: 1);
                        Util.Print("Interact) Space", 2);
                        Util.Print("Speed) + -", 5);
                        new Input.Option()
                            .AddKeybind(new Keybind(() => this.MovePlayer(Direction.Up), keyChar: 'w', key: ConsoleKey.NumPad8))
                            .AddKeybind(new Keybind(() => this.MovePlayer(Direction.Left), keyChar: 'a', key: ConsoleKey.NumPad4))
                            .AddKeybind(new Keybind(() => this.MovePlayer(Direction.Down), keyChar: 's', key: ConsoleKey.NumPad2))
                            .AddKeybind(new Keybind(() => this.MovePlayer(Direction.Right), keyChar: 'd', key: ConsoleKey.NumPad6))
                            .AddKeybind(new Keybind(() => this.Interact(Direction.Up), key: ConsoleKey.UpArrow))
                            .AddKeybind(new Keybind(() => this.Interact(Direction.Left), key: ConsoleKey.LeftArrow))
                            .AddKeybind(new Keybind(() => this.Interact(Direction.Down), key: ConsoleKey.DownArrow))
                            .AddKeybind(new Keybind(() => this.Interact(Direction.Right), key: ConsoleKey.RightArrow))
                            .AddKeybind(new Keybind(() => OptionAdventure.Info.Speed++, key: ConsoleKey.Add))
                            .AddKeybind(new Keybind(() => OptionAdventure.Info.Speed--, key: ConsoleKey.Subtract))
                            .AddKeybind(new Keybind(() =>
                            {
                                this.Save();
                                this._stage = Stage.MainMenu;
                            }, "Quit", key: ConsoleKey.Escape))
                            .Request();
                    }
                    break;
            }
        }

        public sealed override void Save() { Util.Serialize(this._filePath, OptionAdventure.Info); }

        private void InitGame(bool newGame)
        {
            if (newGame)
            {
                OptionAdventure.Info = new AdventureInfo();
                Grid currentGrid = OptionAdventure.CurrentGrid;
                OptionAdventure.Info.Position = new Vector2(
                    currentGrid.Width / 2,
                    currentGrid.Height / 2);
            }
            else
                OptionAdventure.Info = Util.Deserialize<AdventureInfo>(this._filePath);

            Console.Clear();
            this._stage = Stage.Game;
        }

        private void MovePlayer(Direction direction)
        {
            Vector2 newPos;
            Tile tile;
            bool stop = false;

            for (int i = 0; i < OptionAdventure.Info.Speed && !stop; i++)
            {
                newPos = OptionAdventure.Info.Position + (Vector2)direction;
                Grid currentGrid = OptionAdventure.CurrentGrid;

                if (newPos.x >= 0 && newPos.x < currentGrid.Width && newPos.y >= 0 && newPos.y < currentGrid.Height)
                {
                    tile = currentGrid.GetTile(newPos);
                    currentGrid.MoveTo(newPos);
                    Tile.TileTypes tileType = tile.TileType;
                    stop = tileType.StopsMovement() || tileType == Tile.TileTypes.Door;
                    if (!stop) OptionAdventure.Info.Position = newPos;
                }
            }
        }

        private void Interact(Direction direction) { OptionAdventure.CurrentGrid.Interact(OptionAdventure.Info.Position + (Vector2)direction); }

        public static void InitializeGrids()
        {
            // ' ' | EMPTY
            // 'c' | COIN
            // 'd' | DOOR
            // 'w' | WALL
            // 'i' | TILE_INTERACTABLE

            // Initialize Grid Array
            _grids = new Grid[3];

            // Grid 0
            string[] sa = Grid.CreateGrid(new Vector2(15));
            sa[13] = " wwwwwwwwwwwww ";
            sa[12] = "  w         w  ";
            sa[11] = "       i       ";
            sa[7] = "   w       w  d";
            sa[3] = "   w   c   w   ";
            sa[1] = " wwwwwwwwwwwww ";
            OptionAdventure._grids[0] = new Grid(sa);
            OptionAdventure._grids[0].AddInteraction(new Vector2(7, 11), () => OptionAdventure.Message = "You touched it!");

            // Grid 1
            sa = Grid.CreateGrid(new Vector2(17, 21));
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
            OptionAdventure._grids[1] = new Grid(sa);

            // Grid 2
            sa = Grid.CreateGrid(new Vector2(13, 9));
            sa[4] = "           i ";
            OptionAdventure._grids[2] = new Grid(sa);
            OptionAdventure._grids[2].AddInteraction(new Vector2(11, 4), () => OptionAdventure.Message = "The End...?");

            // Add Doors after initializing each room
            OptionAdventure._grids[0].AddDoor(new Vector2(14, 7), new Tuple<int, Vector2>(1, new Vector2(8)));
            OptionAdventure._grids[1].AddDoor(new Vector2(8, 13), new Tuple<int, Vector2>(2, new Vector2(1, 4)));

            // Seal Grids
            foreach (Grid grid in OptionAdventure._grids) grid.Seal();
        }

        private enum Stage
        {
            MainMenu,
            Game,
        }
    }
}
