using System.Collections.Generic;
using System;

/* ================= *\
||                   ||
||        B's        ||
||                   ||
||  Created:         ||
||     2021.11.17    ||
||                   ||
||  Edited:          ||
||     2021.12.10    ||
||                   ||
\* ================= */

public class B
{
    public static bool DebugMode = false;

    public static void Main() { new B().Start(); }

    // The currently selected option
    private Option option = null;
    // Whether or not the program should run
    private bool running = true;

    private void Start()
    {
        Console.TreatControlCAsInput = true;

        while (running)
        {
            try
            {
                Console.Title = "B";
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Util.SetConsoleSize(20, 8);
                Console.Clear();

                InputOptionBuilder.Create("B's")
                    .AddAction('1', () => this.option = new NumberGuesser(), "Number Guesser")
                    .AddAction('2', () => this.option = new Adventure(), "Adventure")
                    .AddSpacer()
                    .AddAction(Util.NULLCHAR, () => this.running = false, "Quit", ConsoleKey.Escape)
                    .Request();

                if (option != null)
                {
                    Util.Print("Starting...", 1);

                    while (option.Running)
                    {
                        option.Loop();
                    }

                    option = null;
                }
            }
            catch (Exception e)
            {
                Util.SetConsoleSize(140, 30);
                Util.Print(e);
                Util.WaitForInput();
                Console.Clear();
            }
        }

        // Place exit code like data saving here
    }
}

public abstract class Option
{
    // Whether the Option should continue to run
    private bool running = true;

    public bool Running { get { return this.running; } }

    public void Quit() { this.running = false; }

    // The method that is called while Option is Running
    public abstract void Loop();
}

public sealed class NumberGuesser : Option
{
    // TODO below
    // make able to specify custom range of number
    // make able to use decimal places
    // make able to use negative numbers

    private readonly string[] winMessages = new string[]
    {
        "Right on!",
        "Perfect!",
        "Correct!",
    };

    private Stage stage = Stage.MainMenu;
    private int numMax = 100;
    private int number;

    public sealed override void Loop()
    {
        switch (this.stage)
        {
            case Stage.MainMenu:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 8);
                    InputOptionBuilder.Create("Number Guesser")
                        .AddAction('1', () => this.stage = Stage.GameSetup, "New Game")
                        .AddSpacer()
                        .AddAction('9', () => this.stage = Stage.Settings, "Settings")
                        .AddAction(Util.NULLCHAR, () => this.Quit(), "Back", ConsoleKey.Escape)
                        .Request();
                }
                break;

            case Stage.GameSetup:
                {
                    this.number = Util.Random.Next(this.numMax) + 1;
                    InputOptionBuilder.ResetNumbersRequestGuess();
                    this.stage = Stage.Game;
                }
                break;

            case Stage.Game:
                {
                    string guessMessage = "Between 0 - " + this.numMax;
                    string guess = InputOptionBuilder.Guess;
                    int guessNum = InputOptionBuilder.GuessNum;
                    bool won = guessNum == this.number;
                    Console.Clear();
                    Util.SetConsoleSize(20, 7);
                    Util.Print();
                    Util.Print(guess, 2);
                    Util.Print();

                    if (guess.Length > 0)
                    {
                        if (guessNum < this.number)
                        {
                            guessMessage = "too low...";
                        }
                        else if (guessNum > this.number)
                        {
                            guessMessage = "TOO HIGH!!!";
                        }
                    }

                    if (won)
                    {
                        guessMessage = this.winMessages[Util.Random.Next(this.winMessages.Length)];
                    }

                    Util.Print(guessMessage, 2);

                    if (won)
                    {
                        Util.WaitForInput();
                        this.stage = Stage.MainMenu;
                    }
                    else
                    {
                        InputOptionBuilder.CreateNumbersRequest("Enter a Number!")
                            .AddAction(Util.NULLCHAR, () => this.Quit(), key: ConsoleKey.Escape)
                            .Request();
                    }
                }
                break;

            case Stage.Settings:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 7);
                    InputOptionBuilder.Create("Settings")
                        .AddAction('1', () => this.stage = Stage.Settings_MaxNumber, "Max Number")
                        .AddSpacer()
                        .AddAction(Util.NULLCHAR, () => this.stage = Stage.MainMenu, "Back", ConsoleKey.Escape)
                        .Request();
                }
                break;

            case Stage.Settings_MaxNumber:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 7);
                    Util.Print();
                    Util.Print("Max - {0}", 1, this.numMax);
                    InputOptionBuilder.CreateNumbersRequest("Enter Max Number")
                        .AddAction(Util.NULLCHAR, () => this.stage = Stage.Settings, "Back", ConsoleKey.Escape)
                        .Request();
                    this.numMax = InputOptionBuilder.GuessNum;
                }
                break;
        }
    }

    private enum Stage
    {
        MainMenu,
        GameSetup,
        Game,
        Settings,
        Settings_MaxNumber,
    }
}

public sealed class Adventure : Option
{
    // Chars
    private const string CHAR_EMPTY = "  ";
    private const string CHAR_PLAYER = "()";
    private const string CHAR_DOOR = "[]";
    private const string CHAR_COIN = "<>";
    private const string CHAR_WALL = "▓▓";
    private const string CHAR_INTERACTABLE = "░░";
    private const string CHAR_BORDER_HORIZONTAL = "==";
    private const string CHAR_BORDER_VERTICAL = "||";
    private const string CHAR_CORNER_A = "//";
    private const string CHAR_CORNER_B = @"\\";

    // Public Variables
    public static Grid CurrentGrid;
    public static string Message = string.Empty;
    public static int Coins
    {
        get { return Adventure.coins; }
        set { Adventure.coins = Math.Max(0, value); }
    }

    // Private Variables
    private static int coins;
    private static Vector2 posPlayer;
    private Stage stage = Stage.MainMenu;
    private int speed = 1;

    private int Speed
    {
        get { return this.speed; }
        set { this.speed = Math.Max(1, value); }
    }

    public sealed override void Loop()
    {
        switch (this.stage)
        {
            case Stage.MainMenu:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 7);
                    InputOptionBuilder.Create("Adventure")
                        // TODO implement "Continue"
                        // TODO implement saving progress (room num, player pos, coins, etc)
                        .AddAction('1', () =>
                        {
                            this.stage = Stage.Game;
                            Grid.InitializeGrids();
                            Adventure.CurrentGrid = Grid.GridFirst;
                            Adventure.ResetPlayerPosition();
                            Adventure.Coins = 0;
                            this.Speed = 1;
                        }, "New Game")
                        .AddSpacer()
                        .AddAction(Util.NULLCHAR, () => this.Quit(), "Back", ConsoleKey.Escape)
                        .Request();
                }
                break;

            case Stage.Game:
                {
                    Console.SetCursorPosition(0, 0);
                    int consoleHeight = Adventure.CurrentGrid.Height + 13;

                    if (B.DebugMode)
                    {
                        Util.Print();
                        // Extra spaces are added to the end to clear leftover text
                        Util.Print("{0,-7}", 1, Adventure.CurrentGrid);
                        Util.Print("Pos: {0,-8}", 1, Adventure.posPlayer);
                        consoleHeight += 3;
                    }

                    Util.SetConsoleSize(Adventure.CurrentGrid.RealWidth + 8, consoleHeight);
                    Util.Print();
                    string borderHorizontal = Util.StringOf(Adventure.CHAR_BORDER_HORIZONTAL, Adventure.CurrentGrid.Width);
                    Util.Print("{0}{1}{2}", 2, Adventure.CHAR_CORNER_A, borderHorizontal, Adventure.CHAR_CORNER_B);
                    string s;
                    Vector2 pos;

                    for (int y = Adventure.CurrentGrid.Height - 1; y >= 0; y--)
                    {
                        s = CHAR_BORDER_VERTICAL;

                        for (int x = 0; x < Adventure.CurrentGrid.Width; x++)
                        {
                            pos = new Vector2(x, y);

                            if (pos == Adventure.posPlayer)
                            {
                                s += Adventure.CHAR_PLAYER;
                            }
                            else if (Adventure.CurrentGrid.HasCoinAt(pos))
                            {
                                s += Adventure.CHAR_COIN;
                            }
                            else
                            {
                                s += Adventure.CurrentGrid.GetTile(pos).Chars;
                            }
                        }

                        Util.Print(s + Adventure.CHAR_BORDER_VERTICAL, 2);
                    }

                    Util.Print("{0}{1}{2}", 2, Adventure.CHAR_CORNER_B, borderHorizontal, Adventure.CHAR_CORNER_A);
                    Util.Print();
                    Util.Print("> {0}", 3, Adventure.Message);
                    Adventure.Message = string.Format("{0,-" + (Adventure.CurrentGrid.RealWidth - 7) + "}", "...");
                    Util.Print();
                    Util.Print("{0,9}: {1,-5}", 0, "Coins", Adventure.Coins);
                    Util.Print("{0,9}: {1,-5}", 0, "Speed", this.Speed);
                    Util.Print();
                    Util.Print("Move) W A S D", 1);
                    InputOptionBuilder.Create()
                        .AddAction('w', () => this.MovePlayer(Direction.Up), key: ConsoleKey.NumPad8)
                        .AddAction('a', () => this.MovePlayer(Direction.Left), key: ConsoleKey.NumPad4)
                        .AddAction('s', () => this.MovePlayer(Direction.Down), key: ConsoleKey.NumPad2)
                        .AddAction('d', () => this.MovePlayer(Direction.Right), key: ConsoleKey.NumPad6)
                        .AddSpacer()
                        .AddAction(Util.NULLCHAR, () => this.stage = Stage.MainMenu, "Quit", ConsoleKey.Escape)
                        .AddAction(Util.NULLCHAR, () => this.Speed++, key: ConsoleKey.Add)
                        .AddAction(Util.NULLCHAR, () => this.Speed--, key: ConsoleKey.Subtract)
                        .Request();
                }
                break;
        }
    }

    private void MovePlayer(Direction direction)
    {
        Vector2 newPos;
        Tile tile;
        bool stop = false;

        for (int i = 0; i < this.Speed && !stop; i++)
        {
            newPos = Adventure.posPlayer + direction.ToVector2();

            if (newPos.x >= 0 && newPos.x < Adventure.CurrentGrid.Width && newPos.y >= 0 && newPos.y < Adventure.CurrentGrid.Height)
            {
                tile = Adventure.CurrentGrid.GetTile(newPos);
                Adventure.CurrentGrid.Interact(newPos);
                stop = tile.StopMovement || tile.IsDoor;

                if (!stop)
                {
                    Adventure.posPlayer = newPos;
                }
            }
        }
    }

    public static void ResetPlayerPosition() { Adventure.posPlayer = new Vector2(Adventure.CurrentGrid.Width / 2, Adventure.CurrentGrid.Height / 2); }

    public sealed class Tile
    {
        public static readonly Dictionary<char, Tile> TileMap = new Dictionary<char, Tile>();

        private static Tile EMPTY = new Tile(Adventure.CHAR_EMPTY);
        private static Tile COIN = new Tile(Adventure.CHAR_EMPTY, coin: true);
        private static Tile DOOR = new Tile(Adventure.CHAR_DOOR, door: true);
        private static Tile WALL = new Tile(Adventure.CHAR_WALL, true);
        private static Tile INTERACTABLE = new Tile(Adventure.CHAR_INTERACTABLE, true, true);

        static Tile()
        {
            Tile.TileMap.Add(' ', Tile.EMPTY);
            Tile.TileMap.Add('c', Tile.COIN);
            Tile.TileMap.Add('d', Tile.DOOR);
            Tile.TileMap.Add('w', Tile.WALL);
            Tile.TileMap.Add('i', Tile.INTERACTABLE);
        }

        public readonly string Chars;
        public readonly bool StopMovement;
        public readonly bool IsInteractable;
        public readonly bool IsCoin;
        public readonly bool IsDoor;

        public Tile(string chars, bool stopMovement = false, bool interactable = false, bool coin = false, bool door = false)
        {
            if (chars.Length != 2) { throw new ArgumentException("chars.Length != 2"); }
            this.Chars = chars;
            this.StopMovement = stopMovement;
            this.IsInteractable = interactable;
            this.IsCoin = coin;
            this.IsDoor = door;
        }

        public sealed override string ToString() { return string.Format("Tile: chars:'{0}', stopMovement: {1}, isInteractable: {2}", this.Chars, this.StopMovement, this.IsInteractable); }

        public static explicit operator Tile(char c)
        {
            try
            {
                return Tile.TileMap[c];
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException(string.Format("Invalid tile character \"{0}\"", c));
            }
        }
    }

    public sealed class Grid
    {
        public static Grid GridFirst { get { return Grid.gridFirst; } }
        public static Grid GridSecond { get { return Grid.gridSecond; } }

        private static Grid gridFirst;
        private static Grid gridSecond;

        public int RealWidth { get { return this.width * 2; } }
        public int Width { get { return this.width; } }
        public int Height { get { return this.height; } }

        private readonly Dictionary<Vector2, Action> interactionList = new Dictionary<Vector2, Action>();
        private readonly Dictionary<Vector2, Grid> doorList = new Dictionary<Vector2, Grid>();
        private readonly List<Vector2> coinList = new List<Vector2>();
        private readonly Tile[][] tileGrid;
        private readonly int width;
        private readonly int height;

        // Private Initialization Cache
        private readonly int initInteractables = 0;
        private readonly int initDoors = 0;
        private bool seald = false;

        public Grid(string[] raw)
        {
            if (raw.Length > 0)
            {
                this.width = raw[0].Length;
                this.height = raw.Length;
                this.tileGrid = new Tile[this.height][];
                string str;
                char[] ca;
                Tile tile;

                for (int y = 0; y < height; y++)
                {
                    str = raw[y];

                    if (str.Length == this.width)
                    {
                        this.tileGrid[y] = new Tile[this.width];
                        ca = str.ToCharArray();

                        for (int x = 0; x < width; x++)
                        {
                            tile = (Tile)ca[x];
                            this.tileGrid[y][x] = tile;

                            if (tile.IsCoin)
                            {
                                this.coinList.Add(new Vector2(x, y));
                            }
                            else if (tile.IsInteractable)
                            {
                                this.initInteractables++;
                            }
                            else if (tile.IsDoor)
                            {
                                this.initDoors++;
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Grid Init Error: Rows must be same length");
                    }
                }
            }
            else
            {
                throw new ArgumentException("Grid Init Error: Must have at least one row");
            }
        }

        public Tile GetTile(Vector2 pos) { return this.GetTile(pos.x, pos.y); }

        public Tile GetTile(int x, int y) { return this.tileGrid[y][x]; }

        public bool HasCoinAt(Vector2 pos) { return this.coinList.Contains(pos); }

        public void AddInteraction(Vector2 pos, Action action) { this.AddFeature<Action>(pos, action, "Interaction", tile => tile.IsInteractable, this.interactionList); }

        public void AddDoor(Vector2 pos, Grid grid) { this.AddFeature<Grid>(pos, grid, "Door", tile => tile.IsDoor, this.doorList); }

        public void Interact(Vector2 pos)
        {
            if (this.seald)
            {
                Tile tile = this.GetTile(pos);

                // Pickup Coin
                if (tile.IsCoin && this.coinList.Contains(pos))
                {
                    this.coinList.Remove(pos);
                    Adventure.Coins++;
                    Adventure.Message = "You picked up a coin!";
                }

                // Check Interactions
                if (tile.IsInteractable && this.interactionList.ContainsKey(pos))
                {
                    this.interactionList[pos]();
                }

                // Check Doors
                if (tile.IsDoor && this.doorList.ContainsKey(pos))
                {
                    Adventure.CurrentGrid = this.doorList[pos];
                    Adventure.ResetPlayerPosition();
                    Console.Clear();
                }
            }
            else
            {
                throw new InvalidOperationException("Interact Error: Cannot interact with unsealed grid");
            }
        }

        public void Seal()
        {
            if (this.initDoors != this.doorList.Count)
            {
                throw new InvalidOperationException("Seal Error: Cannot seal grid with unimplemented doors");
            }

            if (this.initInteractables != this.interactionList.Count)
            {
                throw new InvalidOperationException("Seal Error: Cannot seal grid with unimplemented interactables");
            }

            this.seald = true;
        }

        private void AddFeature<T>(Vector2 pos, T obj, string name, Func<Tile, bool> check, Dictionary<Vector2, T> list)
        {
            if (!this.seald)
            {
                if (check.Invoke(this.GetTile(pos)))
                {
                    if (!list.ContainsKey(pos))
                    {
                        list.Add(pos, obj);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("Add {0} Error: {1} already exists at {2}", name, name, pos));
                    }
                }
                else
                {
                    throw new ArgumentException(string.Format("Add {0} Error: Tile is not {1} - {2}", name, name, pos));
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("Add {0} Error: Cannot add {1} to a sealed grid", name, name));
            }
        }

        public sealed override string ToString() { return string.Format("Grid: {0}x{1}", this.width, this.height); }

        private static string[] CreateGrid(Vector2 dimensions)
        {
            string[] sa = new string[dimensions.y];
            string s = Util.StringOf(" ", dimensions.x);
            for (int i = 0; i < sa.Length; i++) { sa[i] = s; }
            return sa;
        }

        public static void InitializeGrids()
        {
            // ' ' | EMPTY
            // 'c' | COIN
            // 'd' | DOOR
            // 'w' | WALL
            // 'i' | TILE_INTERACTABLE

            // Grid First
            string[] sa = Grid.CreateGrid(new Vector2(15));
            sa[13] = " wwwwwwwwwwwww ";
            sa[12] = "  w         w  ";
            sa[11] = "       i       ";
            sa[7] = "   w       w  d";
            sa[3] = "   w   c   w   ";
            sa[1] = " wwwwwwwwwwwww ";
            Grid.gridFirst = new Grid(sa);
            Grid.gridFirst.AddInteraction(new Vector2(7, 11), () => Adventure.Message = "You touched it!");

            // Grid Second
            sa = Grid.CreateGrid(new Vector2(17, 21));
            sa[15] = "        d        ";
            sa[12] = " www   w w   www ";
            sa[11] = " w    w   w    w ";
            sa[10] = " w             w ";
            sa[9] = " w    w   w    w ";
            sa[8] = " www   w w   www ";
            Grid.gridSecond = new Grid(sa);

            // Add Doors after initializing each room
            Grid.gridFirst.AddDoor(new Vector2(14, 7), Grid.gridSecond);
            Grid.gridSecond.AddDoor(new Vector2(8, 15), Grid.gridFirst);

            // Seal Grids
            Grid.gridFirst.Seal();
            Grid.gridSecond.Seal();
        }
    }

    private enum Stage
    {
        MainMenu,
        Game,
    }
}

public enum Direction
{
    Up,
    Left,
    Down,
    Right,
}

public static class DirectionFunc
{
    public static Vector2 ToVector2(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return Vector2.Up;
            case Direction.Left: return Vector2.Left;
            case Direction.Down: return Vector2.Down;
            default: return Vector2.Right;
        }
    }
}

public sealed class InputOptionBuilder
{
    private readonly List<Tuple<ConsoleKey, char, string, Action>> actions = new List<Tuple<ConsoleKey, char, string, Action>>();
    private string message;

    private static string guess = string.Empty;
    private static int guessNum = 0;

    public static string Guess { get { return InputOptionBuilder.guess; } }
    public static int GuessNum { get { return InputOptionBuilder.guessNum; } }

    private InputOptionBuilder(string message) { this.message = message; }

    public static InputOptionBuilder Create(string message = null) { return new InputOptionBuilder(message); }

    public InputOptionBuilder AddAction(char keyChar, Action action, string description = null, ConsoleKey key = default(ConsoleKey))
    {
        this.actions.Add(new Tuple<ConsoleKey, char, string, Action>(key, keyChar, description, action));
        return this;
    }

    public InputOptionBuilder AddSpacer()
    {
        this.actions.Add(null);
        return this;
    }

    public void Request()
    {
        // Add Debug Keybind
        this.AddAction(Util.NULLCHAR, () =>
        {
            Util.ToggleBool(ref B.DebugMode);
            Console.Clear();
        }, key: ConsoleKey.F12);

        if (this.message != null)
        {
            Util.Print();
            Util.Print("  " + this.message);
        }

        bool printLine = true;
        string s;

        foreach (Tuple<ConsoleKey, char, string, Action> action in this.actions)
        {
            // If action is null, add space in display
            // If action's char or string is null, don't display option
            if (action != null)
            {
                if (action.Item3 != null)
                {
                    if (action.Item2 == Util.NULLCHAR)
                    {
                        s = action.Item1.ToString();
                    }
                    else
                    {
                        s = action.Item2.ToString();
                    }

                    if (printLine)
                    {
                        printLine = false;
                        Util.Print();
                    }

                    Util.Print("{0}) {1}", 1, s, action.Item3);
                }
            }
            else if (!printLine)
            {
                printLine = true;
            }
        }

        // Get User Key Info once, otherwise, it will call different keys each loop
        ConsoleKeyInfo inputKeyInfo = Console.ReadKey(true);

        foreach (Tuple<ConsoleKey, char, string, Action> action in this.actions)
        {
            if (action != null && (action.Item1 == inputKeyInfo.Key || (action.Item2 != Util.NULLCHAR && action.Item2 == inputKeyInfo.KeyChar)))
            {
                action.Item4.Invoke();
                break;
            }
        }

        // This needs to be here for parsing InputOptionBuilder Numbers
        int.TryParse(InputOptionBuilder.guess, out InputOptionBuilder.guessNum);
    }

    public static InputOptionBuilder CreateNumbersRequest(string message)
    {
        InputOptionBuilder iob = InputOptionBuilder.Create(message)
            .AddAction(Util.NULLCHAR, () =>
            {
                InputOptionBuilder.guess = InputOptionBuilder.guess.Substring(0, Math.Max(0, InputOptionBuilder.guess.Length - 1));

                if (!int.TryParse(InputOptionBuilder.guess, out InputOptionBuilder.guessNum))
                {
                    InputOptionBuilder.guessNum = 0;
                }
            }, key: ConsoleKey.Backspace);

        for (int i = 0; i < 10; i++)
        {
            char c = (char)('0' + i);

            iob.AddAction(c, () =>
            {
                if (int.TryParse(InputOptionBuilder.guess + c, out InputOptionBuilder.guessNum))
                {
                    InputOptionBuilder.guess = InputOptionBuilder.guessNum.ToString();
                }
            });
        }

        return iob;
    }

    public static void ResetNumbersRequestGuess()
    {
        InputOptionBuilder.guess = string.Empty;
        InputOptionBuilder.guessNum = 0;
    }
}

public class Vector2
{
    public static readonly Vector2 Up = new Vector2(0, 1);
    public static readonly Vector2 Left = new Vector2(-1, 0);
    public static readonly Vector2 Right = new Vector2(1, 0);
    public static readonly Vector2 Down = new Vector2(0, -1);

    public int x = 0;
    public int y = 0;

    public Vector2(int size) : this(size, size) { }

    public Vector2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override int GetHashCode() { return (this.x * this.y).GetHashCode(); }

    public override bool Equals(object obj) { return obj is Vector2 && this == (Vector2)obj; }

    public static Vector2 operator +(Vector2 vecA, Vector2 vecB) { return new Vector2(vecA.x + vecB.x, vecA.y + vecB.y); }

    public static bool operator ==(Vector2 vecA, Vector2 vecB) { return vecA.x == vecB.x && vecA.y == vecB.y; }

    public static bool operator !=(Vector2 vecA, Vector2 vecB) { return !(vecA == vecB); }

    public sealed override string ToString() { return string.Format("({0}, {1})", this.x, this.y); }
}

public static class Util
{
    public const char NULLCHAR = default(char);

    public static readonly Random Random = new Random();

    public static void WaitForInput() { Console.ReadKey(true); }

    public static string StringOf(string str, int repeat)
    {
        string s = string.Empty;

        for (int i = 0; i < repeat; i++)
        {
            s += str;
        }

        return s;
    }

    public static void Print(object message = null, int offsetLeft = 0, params object[] args)
    {
        // If no message, cannot print
        if (message == null)
        {
            Console.WriteLine();
        }
        else
        {
            string messageStr = string.Format(message.ToString(), args);

            for (int i = 0; i < offsetLeft; i++)
            {
                messageStr = " " + messageStr;
            }

            Console.WriteLine(messageStr);
        }
    }

    public static void SetConsoleSize(int width, int height)
    {
        Console.SetWindowSize(width, height);
        Console.SetBufferSize(Console.WindowLeft + width, Console.WindowTop + height);
        Console.SetWindowSize(width, height);
    }

    public static void ToggleBool(ref bool b) { b = !b; }
}
