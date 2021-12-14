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
||     2021.12.13    ||
||                   ||
\* ================= */

public class B
{
    public static bool DebugMode = false;

    public static void Main() { new B().Start(); }

    // The currently selected option
    private Option _option = null;
    // Whether or not the program should run
    private bool _running = true;

    private void Start()
    {
        Console.TreatControlCAsInput = true;

        while (this._running)
        {
            try
            {
                if (this._option != null && this._option.IsRunning)
                    this._option.Loop();
                else
                {
                    Console.Title = "B";
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Clear();
                    Util.SetConsoleSize(20, 8);
                    InputOptionBuilder.Create("B's")
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this._option = new NumberGuesser(), "Number Guesser", '1'))
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this._option = new Adventure(), "Adventure", '2'))
                        .AddSpacer()
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this._running = false, "Quit", key: ConsoleKey.Escape))
                        .Request();
                }
            }
            catch (Exception e)
            {
                Util.SetConsoleSize(140, 30);
                Util.Print(e);
                Util.WaitForKey(ConsoleKey.F1);
                Console.Clear();
            }
        }

        // TODO make exit code run upon Option exit, this should be overridden function in Option for easy implementation
        // Place exit code like data saving here
    }
}

public abstract class Option
{
    // Whether the Option should continue to run
    private bool _running = true;

    public bool IsRunning { get { return this._running; } }

    public void Quit() { this._running = false; }

    // The method that is called while Option is Running
    public abstract void Loop();
}

public sealed class NumberGuesser : Option
{
    // TODO below
    // make able to specify custom range of number
    // make able to use decimal places
    // make able to use negative numbers

    private static readonly string[] _winMessages = new string[]
    {
        "Right on!",
        "Perfect!",
        "Correct!",
    };

    private Stage _stage = Stage.MainMenu;
    private int _numMax = 100;
    private int _number;

    public sealed override void Loop()
    {
        switch (this._stage)
        {
            case Stage.MainMenu:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 8);
                    InputOptionBuilder.Create("Number Guesser")
                        .AddKeybind(new InputOptionBuilder.Keybind(() =>
                        {
                            this._number = Util.Random.Next(this._numMax) + 1;
                            InputOptionBuilder.ResetNumbersRequestGuess();
                            this._stage = Stage.Game;
                        }, "New Game", '1'))
                        .AddSpacer()
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this._stage = Stage.Settings, "Settings", '9'))
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this.Quit(), "Back", key: ConsoleKey.Escape))
                        .Request();
                }
                break;

            case Stage.Game:
                {
                    string guessMessage = "Between 0 - " + this._numMax;
                    string guess = InputOptionBuilder.Guess;
                    int guessNum = InputOptionBuilder.GuessNum;
                    bool won = guessNum == this._number;
                    Console.Clear();
                    int consoleHeight = 7;

                    if (B.DebugMode)
                    {
                        Util.Print();
                        Util.Print(string.Format("Number: {0,-3}", this._number), 1);
                        Util.Print();
                        consoleHeight += 3;
                    }

                    Util.SetConsoleSize(20, consoleHeight);
                    Util.Print();
                    Util.Print(guess, 2);
                    Util.Print();
                    guessMessage = guess.Length == 0 ? "..." :
                        won ? NumberGuesser._winMessages[Util.Random.Next(NumberGuesser._winMessages.Length)] :
                            guessNum < this._number ? "too low..." : "TOO HIGH!!!";

                    Util.Print(guessMessage, 2);

                    if (won)
                    {
                        Util.WaitForInput();
                        this._stage = Stage.MainMenu;
                    }
                    else
                    {
                        InputOptionBuilder.CreateNumbersRequest("Enter a Number!")
                            .AddKeybind(new InputOptionBuilder.Keybind(() => this._stage = Stage.MainMenu, key: ConsoleKey.Escape))
                            .Request();
                    }
                }
                break;

            case Stage.Settings:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 7);
                    InputOptionBuilder.Create("Settings")
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this._stage = Stage.Settings_MaxNumber, "Max Number", '1'))
                        .AddSpacer()
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this._stage = Stage.MainMenu, "Back", key: ConsoleKey.Escape))
                        .Request();
                }
                break;

            case Stage.Settings_MaxNumber:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 7);
                    Util.Print();
                    Util.Print(string.Format("Max - {0}", this._numMax), 1);
                    InputOptionBuilder.CreateNumbersRequest("Enter Max Number")
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this._stage = Stage.Settings, "Back", key: ConsoleKey.Escape))
                        .Request();
                    this._numMax = InputOptionBuilder.GuessNum;
                }
                break;
        }
    }

    private enum Stage
    {
        MainMenu,
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
    private const string MESSAGE_EMPTY = "...";

    // Public Variables
    public static Grid CurrentGrid;
    public static string Message = Adventure.MESSAGE_EMPTY;
    public static int Coins
    {
        get { return Adventure.coins; }
        set { Adventure.coins = Math.Max(0, value); }
    }

    // Private Variables
    private static int coins;
    private static Vector2 posPlayer;

    private Stage _stage = Stage.MainMenu;
    private int _speed = 1;

    private int Speed
    {
        get { return this._speed; }
        set { this._speed = Math.Max(1, value); }
    }

    public sealed override void Loop()
    {
        switch (this._stage)
        {
            case Stage.MainMenu:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 7);
                    InputOptionBuilder.Create("Adventure")
                        // TODO implement "Continue"
                        // TODO implement saving progress (room num, player pos, coins, etc)
                        .AddKeybind(new InputOptionBuilder.Keybind(() =>
                        {
                            this._stage = Stage.Game;
                            Grid.InitializeGrids();
                            Adventure.CurrentGrid = Grid.GridFirst;
                            Adventure.ResetPlayerPosition();
                            Adventure.Coins = 0;
                            this.Speed = 1;
                            Console.Clear();
                        }, "New Game", '1'))
                        .AddSpacer()
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this.Quit(), "Back", key: ConsoleKey.Escape))
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
                        Util.Print(string.Format("{0,-7}", Adventure.CurrentGrid), 1);
                        Util.Print(string.Format("Pos: {0,-8}", Adventure.posPlayer), 1);
                        consoleHeight += 3;
                    }

                    Util.SetConsoleSize(Adventure.CurrentGrid.RealWidth + 8, consoleHeight);
                    Util.Print();
                    string borderHorizontal = Util.StringOf(Adventure.CHAR_BORDER_HORIZONTAL, Adventure.CurrentGrid.Width);
                    Util.Print(string.Format("{0}{1}{2}", Adventure.CHAR_CORNER_A, borderHorizontal, Adventure.CHAR_CORNER_B), 2);
                    string s;
                    Vector2 pos;

                    for (int y = Adventure.CurrentGrid.Height - 1; y >= 0; y--)
                    {
                        s = CHAR_BORDER_VERTICAL;

                        for (int x = 0; x < Adventure.CurrentGrid.Width; x++)
                        {
                            pos = new Vector2(x, y);

                            if (pos == Adventure.posPlayer)
                                s += Adventure.CHAR_PLAYER;
                            else if (Adventure.CurrentGrid.HasCoinAt(pos))
                                s += Adventure.CHAR_COIN;
                            else
                                s += Adventure.CurrentGrid.GetTile(pos).Chars;
                        }

                        Util.Print(s + Adventure.CHAR_BORDER_VERTICAL, 2);
                    }

                    Util.Print(string.Format("{0}{1}{2}", Adventure.CHAR_CORNER_B, borderHorizontal, Adventure.CHAR_CORNER_A), 2);
                    Util.Print();
                    Util.Print(string.Format("> {0}", Adventure.Message), 3);
                    Adventure.Message = string.Format("{0,-" + (Adventure.CurrentGrid.RealWidth - 7) + "}", Adventure.MESSAGE_EMPTY);
                    Util.Print();
                    string format = "{0,9}: {1,-5}";
                    Util.Print(string.Format(format, "Coins", Adventure.Coins));
                    Util.Print(string.Format(format, "Speed", this.Speed));
                    Util.Print();
                    Util.Print("Move) W A S D", 1);
                    InputOptionBuilder.Create()
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this.MovePlayer(Direction.Up), keyChar: 'w', key: ConsoleKey.NumPad8))
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this.MovePlayer(Direction.Left), keyChar: 'a', key: ConsoleKey.NumPad4))
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this.MovePlayer(Direction.Down), keyChar: 's', key: ConsoleKey.NumPad2))
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this.MovePlayer(Direction.Right), keyChar: 'd', key: ConsoleKey.NumPad6))
                        .AddSpacer()
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this._stage = Stage.MainMenu, "Quit", key: ConsoleKey.Escape))
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this.Speed++, key: ConsoleKey.Add))
                        .AddKeybind(new InputOptionBuilder.Keybind(() => this.Speed--, key: ConsoleKey.Subtract))
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
                    Adventure.posPlayer = newPos;
            }
        }
    }

    public static void ResetPlayerPosition() { Adventure.posPlayer = new Vector2(Adventure.CurrentGrid.Width / 2, Adventure.CurrentGrid.Height / 2); }

    public sealed class Tile
    {
        private static readonly Dictionary<char, Tile> TileMap = new Dictionary<char, Tile>();
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
            try { return Tile.TileMap[c]; }
            catch (KeyNotFoundException) { throw new ArgumentException(string.Format("Invalid tile character \"{0}\"", c)); }
        }
    }

    public sealed class Grid
    {
        public static Grid GridFirst { get { return Grid.gridFirst; } }
        public static Grid GridSecond { get { return Grid.gridSecond; } }

        private static Grid gridFirst;
        private static Grid gridSecond;

        public int RealWidth { get { return this._width * 2; } }
        public int Width { get { return this._width; } }
        public int Height { get { return this._height; } }

        private readonly Dictionary<Vector2, Action> _interactionList = new Dictionary<Vector2, Action>();
        private readonly Dictionary<Vector2, Grid> _doorList = new Dictionary<Vector2, Grid>();
        private readonly List<Vector2> _coinList = new List<Vector2>();
        private readonly Tile[][] _tileGrid;
        private readonly int _width;
        private readonly int _height;

        // Private Initialization Cache
        private readonly int _initInteractables = 0;
        private readonly int _initDoors = 0;
        private bool _seald = false;

        public Grid(string[] raw)
        {
            if (raw.Length > 0)
            {
                this._width = raw[0].Length;
                this._height = raw.Length;
                this._tileGrid = new Tile[this._height][];
                string str;
                char[] ca;
                Tile tile;

                for (int y = 0; y < _height; y++)
                {
                    str = raw[y];

                    if (str.Length == this._width)
                    {
                        this._tileGrid[y] = new Tile[this._width];
                        ca = str.ToCharArray();

                        for (int x = 0; x < _width; x++)
                        {
                            tile = (Tile)ca[x];
                            this._tileGrid[y][x] = tile;

                            if (tile.IsCoin)
                                this._coinList.Add(new Vector2(x, y));
                            else if (tile.IsInteractable)
                                this._initInteractables++;
                            else if (tile.IsDoor)
                                this._initDoors++;
                        }
                    }
                    else
                        throw new ArgumentException("Grid Init Error: Rows must be same length");
                }
            }
            else
                throw new ArgumentException("Grid Init Error: Must have at least one row");
        }

        public Tile GetTile(Vector2 pos) { return this.GetTile(pos.x, pos.y); }

        public Tile GetTile(int x, int y) { return this._tileGrid[y][x]; }

        public bool HasCoinAt(Vector2 pos) { return this._coinList.Contains(pos); }

        public void AddInteraction(Vector2 pos, Action action) { this.AddFeature(pos, action, "Interaction", tile => tile.IsInteractable, this._interactionList); }

        public void AddDoor(Vector2 pos, Grid grid) { this.AddFeature(pos, grid, "Door", tile => tile.IsDoor, this._doorList); }

        public void Interact(Vector2 pos)
        {
            if (this._seald)
            {
                Tile tile = this.GetTile(pos);

                // Pickup Coin
                if (tile.IsCoin && this._coinList.Contains(pos))
                {
                    this._coinList.Remove(pos);
                    Adventure.Coins++;
                    Adventure.Message = "You picked up a coin!";
                }

                // Check Interactions
                if (tile.IsInteractable && this._interactionList.ContainsKey(pos))
                    this._interactionList[pos]();

                // Check Doors
                if (tile.IsDoor && this._doorList.ContainsKey(pos))
                {
                    Adventure.CurrentGrid = this._doorList[pos];
                    Adventure.ResetPlayerPosition();
                    Console.Clear();
                }
            }
            else
                throw new InvalidOperationException("Interact Error: Cannot interact with unsealed grid");
        }

        public void Seal()
        {
            if (this._initDoors != this._doorList.Count)
                throw new InvalidOperationException("Seal Error: Cannot seal grid with unimplemented doors");

            if (this._initInteractables != this._interactionList.Count)
                throw new InvalidOperationException("Seal Error: Cannot seal grid with unimplemented interactables");

            this._seald = true;
        }

        private void AddFeature<T>(Vector2 pos, T obj, string name, Func<Tile, bool> check, Dictionary<Vector2, T> list)
        {
            if (!this._seald)
            {
                if (check.Invoke(this.GetTile(pos)))
                {
                    if (!list.ContainsKey(pos))
                        list.Add(pos, obj);
                    else
                        throw new InvalidOperationException(string.Format("Add {0} Error: {1} already exists at {2}", name, name, pos));
                }
                else
                    throw new ArgumentException(string.Format("Add {0} Error: Tile is not {1} - {2}", name, name, pos));
            }
            else
                throw new InvalidOperationException(string.Format("Add {0} Error: Cannot add {1} to a sealed grid", name, name));
        }

        public sealed override string ToString() { return string.Format("Grid: {0}x{1}", this._width, this._height); }

        private static string[] CreateGrid(Vector2 dimensions)
        {
            string[] sa = new string[dimensions.y];
            string s = Util.StringOf(" ", dimensions.x);
            for (int i = 0; i < sa.Length; i++) sa[i] = s;
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
            sa[14] = " www         www ";
            sa[13] = " w             w ";
            sa[12] = " w     w w     w ";
            sa[11] = " w    w   w    w ";
            sa[10] = " w             w ";
            sa[9] = " w    w   w    w ";
            sa[8] = " w     w w     w ";
            sa[7] = " w             w ";
            sa[6] = " www         www ";
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
            case Direction.Right: return Vector2.Right;
        }

        return Vector2.Zero;
    }
}

public sealed class InputOptionBuilder
{
    private static string guess = string.Empty;
    private static int guessNum = 0;

    public static string Guess { get { return InputOptionBuilder.guess; } }
    public static int GuessNum { get { return InputOptionBuilder.guessNum; } }

    private readonly List<Keybind> _keybinds = new List<Keybind>();
    private string _message;

    private InputOptionBuilder(string message) { this._message = message; }

    public static InputOptionBuilder Create(string message = null) { return new InputOptionBuilder(message); }

    public InputOptionBuilder AddKeybind(Keybind keybind)
    {
        this._keybinds.Add(keybind);
        return this;
    }

    public InputOptionBuilder AddSpacer() { return this.AddKeybind(null); }

    public void Request()
    {
        // Add Debug Keybind
        this.AddKeybind(new Keybind(() =>
        {
            Util.ToggleBool(ref B.DebugMode);
            Console.Clear();
        }, key: ConsoleKey.F12));

        if (this._message != null)
        {
            Util.Print();
            Util.Print(this._message, 2);
        }

        bool printLine = true;
        string s;

        foreach (Keybind keybind in this._keybinds)
        {
            // If action is null, add space in display
            // If action's char or string is null, don't display option
            if (keybind != null)
            {
                if (keybind.Description != null)
                {
                    s = keybind.KeyChar == Util.NULLCHAR ? keybind.Key.ToString() : keybind.KeyChar.ToString();

                    if (printLine)
                    {
                        printLine = false;
                        Util.Print();
                    }

                    Util.Print(string.Format("{0}) {1}", s, keybind.Description), 1);
                }
            }
            else if (!printLine)
                printLine = true;
        }

        // Get User Key Info once, otherwise, it will call different keys each loop
        ConsoleKeyInfo inputKeyInfo = Util.GetInput();

        foreach (Keybind keybind in this._keybinds)
        {
            if (keybind != null && (keybind.Key == inputKeyInfo.Key || (keybind.KeyChar != Util.NULLCHAR && keybind.KeyChar == inputKeyInfo.KeyChar)))
            {
                keybind.Action.Invoke();
                break;
            }
        }

        // This needs to be here for parsing InputOptionBuilder Numbers
        int.TryParse(InputOptionBuilder.guess, out InputOptionBuilder.guessNum);
    }

    public static InputOptionBuilder CreateNumbersRequest(string message)
    {
        InputOptionBuilder iob = InputOptionBuilder.Create(message)
            .AddKeybind(new Keybind(() =>
            {
                InputOptionBuilder.guess = InputOptionBuilder.guess.Substring(0, Math.Max(0, InputOptionBuilder.guess.Length - 1));

                if (!int.TryParse(InputOptionBuilder.guess, out InputOptionBuilder.guessNum))
                    InputOptionBuilder.guessNum = 0;
            }, key: ConsoleKey.Backspace));

        for (int i = 0; i < 10; i++)
        {
            char c = (char)('0' + i);

            iob.AddKeybind(new Keybind(() =>
            {
                if (int.TryParse(InputOptionBuilder.guess + c, out InputOptionBuilder.guessNum))
                    InputOptionBuilder.guess = InputOptionBuilder.guessNum.ToString();
            }, keyChar: c));
        }

        return iob;
    }

    public static void ResetNumbersRequestGuess()
    {
        InputOptionBuilder.guess = string.Empty;
        InputOptionBuilder.guessNum = 0;
    }

    public sealed class Keybind
    {
        public readonly ConsoleKey Key;
        public readonly char KeyChar;
        public readonly string Description;
        public readonly Action Action;

        public Keybind(Action action, string description = null, char keyChar = Util.NULLCHAR, ConsoleKey key = default(ConsoleKey))
        {
            this.KeyChar = keyChar;
            this.Key = key;
            this.Description = description;
            this.Action = action;
        }
    }
}

public class Vector2
{
    public static readonly Vector2 Up = new Vector2(0, 1);
    public static readonly Vector2 Left = new Vector2(-1, 0);
    public static readonly Vector2 Right = new Vector2(1, 0);
    public static readonly Vector2 Down = new Vector2(0, -1);
    public static readonly Vector2 Zero = new Vector2(0);

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

    public static void WaitForInput() { Util.GetInput(); }

    public static ConsoleKeyInfo GetInput() { return Console.ReadKey(true); }

    public static string GetLine() { return Console.ReadLine(); }

    public static void WaitForKey(ConsoleKey key, bool displayMessage = true)
    {
        if (displayMessage)
        {
            Util.Print();
            Util.Print(string.Format("Press {0} to continue...", key), 0);
        }

        bool keyPressed = false;

        while (!keyPressed)
            if (Util.GetInput().Key == key)
                keyPressed = true;
    }

    public static string StringOf(string str, int repeat)
    {
        string s = string.Empty;

        for (int i = 0; i < repeat; i++)
            s += str;

        return s;
    }

    public static void Print(object message = null, int offsetLeft = 0, bool newLine = true)
    {
        string messageStr = (String)message ?? string.Empty;

        for (int i = 0; i < offsetLeft; i++)
            messageStr = " " + messageStr;

        if (newLine)
            Console.WriteLine(messageStr);
        else
            Console.Write(messageStr);
    }

    public static void SetConsoleSize(int width, int height)
    {
        Console.SetWindowSize(width, height);
        Console.SetBufferSize(Console.WindowLeft + width, Console.WindowTop + height);
        Console.SetWindowSize(width, height);
    }

    public static void ToggleBool(ref bool b) { b = !b; }
}
