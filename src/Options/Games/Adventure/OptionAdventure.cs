using B.Inputs;
using B.Utils;
using B.Utils.Extensions;

namespace B.Options.Games.Adventure
{
    public sealed class OptionAdventure : Option<OptionAdventure.Stages>
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

        public static string Title => "Adventure!";

        public static readonly string FilePath = Program.DataPath + "adventure";

        public static string Message = OptionAdventure.MESSAGE_EMPTY;
        public static AdventureInfo Info = new();

        public static Grid CurrentGrid => OptionAdventure._grids[OptionAdventure.Info.GridID];
        private static Grid[] _grids = new Grid[0];

        private Vector2 PlayerPosition
        {
            get => OptionAdventure.Info.Position;
            set => OptionAdventure.Info.Position = value;
        }

        static OptionAdventure() => OptionAdventure.InitializeGrids();

        public OptionAdventure() : base(Stages.MainMenu) { }

        // TODO implement some color printing (examples: tiles, player, coins, doors, border, text, etc.)

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        bool fileExists = File.Exists(OptionAdventure.FilePath);
                        Window.Clear();
                        Window.SetSize(20, fileExists ? 8 : 7);
                        Cursor.Position = new(0, 1);
                        Input.Choice choice = Input.Choice.Create(OptionAdventure.Title);
                        if (fileExists)
                        {
                            choice.AddKeybind(Keybind.CreateConfirmation(() => InitGame(true), "Are you sure you want to override the current game?", "New Game", '1'));
                            choice.AddKeybind(Keybind.Create(() => InitGame(false), "Continue", '2'));
                        }
                        else
                        {
                            choice.AddKeybind(Keybind.Create(() => InitGame(true), "New Game", '1'));
                        }
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
                                OptionAdventure.Info = Data.Deserialize<AdventureInfo>(OptionAdventure.FilePath)!;

                            Window.Clear();
                            SetStage(Stages.Game);
                        }
                    }
                    break;

                case Stages.Game:
                    {
                        // TODO rewrite and take into account different border styles

                        Cursor.Reset();
                        Grid currentGrid = OptionAdventure.CurrentGrid;
                        int consoleHeight = currentGrid.Height + 15;

                        if (Program.Settings.DebugMode.Active)
                        {
                            // Extra spaces are added to the end to clear leftover text
                            Window.PrintLine();
                            Window.PrintLine($" {currentGrid,-7}");
                            Window.PrintLine($" Pos: {PlayerPosition,-8}");
                            consoleHeight += 3;
                        }

                        Window.SetSize(currentGrid.RealWidth + 8, consoleHeight);
                        string borderHorizontal = OptionAdventure.CHAR_BORDER_HORIZONTAL.Loop(currentGrid.Width);
                        Window.PrintLine();
                        Window.PrintLine($"  {OptionAdventure.CHAR_CORNER_A}{borderHorizontal}{OptionAdventure.CHAR_CORNER_B}");

                        for (int y = currentGrid.Height - 1; y >= 0; y--)
                        {
                            string s = CHAR_BORDER_VERTICAL;

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

                            Window.PrintLine($"  {s + OptionAdventure.CHAR_BORDER_VERTICAL}");
                        }

                        Window.PrintLine($"  {OptionAdventure.CHAR_CORNER_B}{borderHorizontal}{OptionAdventure.CHAR_CORNER_A}");
                        Window.PrintLine();
                        Window.PrintLine($"   > {OptionAdventure.Message}");
                        OptionAdventure.Message = string.Format("{0,-" + (currentGrid.RealWidth - 7) + "}", OptionAdventure.MESSAGE_EMPTY);
                        Window.PrintLine();
                        string format = "{0,9}: {1,-5}";
                        Window.PrintLine(string.Format(format, "Coins", OptionAdventure.Info.Coins));
                        Window.PrintLine(string.Format(format, "Speed", OptionAdventure.Info.Speed));
                        Window.PrintLine();
                        Window.PrintLine("      Move) W A S D");
                        Window.PrintLine("  Interact) Space");
                        Window.PrintLine("     Speed) + -");
                        Input.Choice choice = Input.Choice.Create();
                        // TODO instead of creating and adding new keybinds every time, try creating and caching this in the constructor and just request it when necessary
                        choice.AddKeybind(Keybind.Create(() => MovePlayer(Direction.Up), keyChar: 'w', key: ConsoleKey.NumPad8));
                        choice.AddKeybind(Keybind.Create(() => MovePlayer(Direction.Left), keyChar: 'a', key: ConsoleKey.NumPad4));
                        choice.AddKeybind(Keybind.Create(() => MovePlayer(Direction.Down), keyChar: 's', key: ConsoleKey.NumPad2));
                        choice.AddKeybind(Keybind.Create(() => MovePlayer(Direction.Right), keyChar: 'd', key: ConsoleKey.NumPad6));
                        choice.AddKeybind(Keybind.Create(() => Interact(Direction.Up), key: ConsoleKey.UpArrow));
                        choice.AddKeybind(Keybind.Create(() => Interact(Direction.Left), key: ConsoleKey.LeftArrow));
                        choice.AddKeybind(Keybind.Create(() => Interact(Direction.Down), key: ConsoleKey.DownArrow));
                        choice.AddKeybind(Keybind.Create(() => Interact(Direction.Right), key: ConsoleKey.RightArrow));
                        choice.AddKeybind(Keybind.Create(() => OptionAdventure.Info.Speed++, key: ConsoleKey.Add));
                        choice.AddKeybind(Keybind.Create(() => OptionAdventure.Info.Speed--, key: ConsoleKey.Subtract));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.Create(() =>
                        {
                            Save();
                            SetStage(Stages.MainMenu);
                        }, "Quit", key: ConsoleKey.Escape));
                        choice.Request();
                    }
                    break;
            }
        }

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

        public override void Save() => Data.Serialize(OptionAdventure.FilePath, OptionAdventure.Info);

        public enum Stages
        {
            MainMenu,
            Game,
        }
    }
}
