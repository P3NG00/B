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
        public static AdventureInfo Info = new();

        public static Grid CurrentGrid => OptionAdventure._grids[OptionAdventure.Info.GridID];
        private static Grid[] _grids = new Grid[0];

        private readonly string _filePath = Program.DataPath + "adventureInfo";
        private Stage _stage = Stage.MainMenu;

        static OptionAdventure() => OptionAdventure.InitializeGrids();

        public sealed override void Loop()
        {
            switch (this._stage)
            {
                case Stage.MainMenu:
                    {
                        int consoleHeight = 7;
                        bool fileExists = File.Exists(this._filePath);

                        if (fileExists)
                            consoleHeight++;

                        Util.ClearConsole(20, consoleHeight);
                        Input.Option iob = new Input.Option("Adventure")
                            .Add(() => this.InitGame(true), "New Game", '1');

                        if (fileExists)
                            iob.Add(() => this.InitGame(false), "Continue", '2');

                        iob.AddSpacer()
                            .Add(() => this.Quit(), "Exit", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stage.Game:
                    {
                        Util.ResetTextCursor();
                        Grid currentGrid = OptionAdventure.CurrentGrid;
                        int consoleHeight = currentGrid.Height + 15;

                        if (Program.Settings.DebugMode)
                        {
                            // Extra spaces are added to the end to clear leftover text
                            Util.PrintLine();
                            Util.PrintLine($" {currentGrid,-7}");
                            Util.PrintLine($" Pos: {OptionAdventure.Info.Position,-8}");
                            consoleHeight += 3;
                        }

                        Util.SetConsoleSize(currentGrid.RealWidth + 8, consoleHeight);
                        string borderHorizontal = Util.StringOf(OptionAdventure.CHAR_BORDER_HORIZONTAL, currentGrid.Width);
                        Util.PrintLine();
                        Util.PrintLine($"  {OptionAdventure.CHAR_CORNER_A}{borderHorizontal}{OptionAdventure.CHAR_CORNER_B}");

                        for (int y = currentGrid.Height - 1; y >= 0; y--)
                        {
                            string s = CHAR_BORDER_VERTICAL;

                            for (int x = 0; x < currentGrid.Width; x++)
                            {
                                Vector2 pos = new(x, y);

                                if (pos == OptionAdventure.Info.Position)
                                    s += OptionAdventure.CHAR_PLAYER;
                                else if (currentGrid.HasCoinAt(pos))
                                    s += OptionAdventure.CHAR_COIN;
                                else
                                    s += currentGrid.GetTile(pos).Chars;
                            }

                            Util.PrintLine($"  {s + OptionAdventure.CHAR_BORDER_VERTICAL}");
                        }

                        Util.PrintLine($"  {OptionAdventure.CHAR_CORNER_B}{borderHorizontal}{OptionAdventure.CHAR_CORNER_A}");
                        Util.PrintLine();
                        Util.PrintLine($"   > {OptionAdventure.Message}");
                        OptionAdventure.Message = string.Format("{0,-" + (currentGrid.RealWidth - 7) + "}", OptionAdventure.MESSAGE_EMPTY);
                        Util.PrintLine();
                        string format = "{0,9}: {1,-5}";
                        Util.PrintLine(string.Format(format, "Coins", OptionAdventure.Info.Coins));
                        Util.PrintLine(string.Format(format, "Speed", OptionAdventure.Info.Speed));
                        Util.PrintLine();
                        Util.PrintLine("      Move) W A S D");
                        Util.PrintLine("  Interact) Space");
                        Util.PrintLine("     Speed) + -");
                        new Input.Option()
                            .Add(() => this.MovePlayer(Direction.Up), keyChar: 'w', key: ConsoleKey.NumPad8)
                            .Add(() => this.MovePlayer(Direction.Left), keyChar: 'a', key: ConsoleKey.NumPad4)
                            .Add(() => this.MovePlayer(Direction.Down), keyChar: 's', key: ConsoleKey.NumPad2)
                            .Add(() => this.MovePlayer(Direction.Right), keyChar: 'd', key: ConsoleKey.NumPad6)
                            .Add(() => this.Interact(Direction.Up), key: ConsoleKey.UpArrow)
                            .Add(() => this.Interact(Direction.Left), key: ConsoleKey.LeftArrow)
                            .Add(() => this.Interact(Direction.Down), key: ConsoleKey.DownArrow)
                            .Add(() => this.Interact(Direction.Right), key: ConsoleKey.RightArrow)
                            .Add(() => OptionAdventure.Info.Speed++, key: ConsoleKey.Add)
                            .Add(() => OptionAdventure.Info.Speed--, key: ConsoleKey.Subtract)
                            .Add(() =>
                            {
                                this.Save();
                                this._stage = Stage.MainMenu;
                            }, "Quit", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;
            }
        }

        public sealed override void Save() => Util.Serialize(this._filePath, OptionAdventure.Info);

        private void InitGame(bool newGame)
        {
            if (newGame)
            {
                OptionAdventure.Info = new();
                Grid currentGrid = OptionAdventure.CurrentGrid;
                OptionAdventure.Info.Position = new(currentGrid.Width / 2, currentGrid.Height / 2);
            }
            else
                OptionAdventure.Info = Util.Deserialize<AdventureInfo>(this._filePath)!;

            Util.ClearConsole();
            this._stage = Stage.Game;
        }

        private void MovePlayer(Direction direction)
        {
            bool stop = false;

            for (int i = 0; i < OptionAdventure.Info.Speed && !stop; i++)
            {
                Vector2 newPos = OptionAdventure.Info.Position + (Vector2)direction;
                Grid currentGrid = OptionAdventure.CurrentGrid;

                if (newPos.x >= 0 && newPos.x < currentGrid.Width && newPos.y >= 0 && newPos.y < currentGrid.Height)
                {
                    Tile tile = currentGrid.GetTile(newPos);
                    currentGrid.MoveTo(newPos);
                    Tile.TileTypes tileType = tile.TileType;
                    stop = tileType.StopsMovement() || tileType == Tile.TileTypes.Door;

                    if (!stop)
                        OptionAdventure.Info.Position = newPos;
                }
            }
        }

        private void Interact(Direction direction) => OptionAdventure.CurrentGrid.Interact(OptionAdventure.Info.Position + (Vector2)direction);

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

            // Seal Grids
            foreach (Grid grid in OptionAdventure._grids)
                grid.Seal();
        }

        private enum Stage
        {
            MainMenu,
            Game,
        }
    }
}
