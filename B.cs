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
||     2021.12.05    ||
||                   ||
\* ================= */

/*

TODO options
Solitaire
Minesweeper
Blackjack

*/

public class Program
{
    // TODO switch to false when done implementing
    public static bool DebugMode = false; // TODO implement way of enable during runtime

    public static void Main() { new Program().Start(); }

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
    // make able to specify custom range of number/
    // make able to use decimal places
    // make able to use negative numbers

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
                        string[] winMsgs = new string[]
                        {
                            "Right on!",
                            "Perfect!",
                            "Correct!",
                        };

                        guessMessage = winMsgs[Util.Random.Next(winMsgs.Length)];
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
    private const string CHAR_PLAYER = "()";
    private const string CHAR_ENEMY = "[]";
    private const string CHAR_COIN = "<>";
    private const string CHAR_BORDER_HORIZONTAL = "==";
    private const string CHAR_BORDER_VERTICAL = "||";
    private const string CHAR_CORNER_A = "//";
    private const string CHAR_CORNER_B = @"\\";

    // Public Variables
    public static string Message = string.Empty;
    public static int Coins
    {
        get { return Adventure.coins; }
        set { Adventure.coins = Math.Max(0, value); }
    }

    // Private Variables
    private static int coins = 0;
    private Stage stage = Stage.MainMenu;
    private Vector2 posPlayer;
    private Grid grid;

    public sealed override void Loop()
    {
        switch (this.stage)
        {
            case Stage.MainMenu:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 7);
                    InputOptionBuilder.Create("Adventure")
                        .AddAction('1', () => this.stage = Stage.GameSetup, "New Game")
                        .AddSpacer()
                        .AddAction(Util.NULLCHAR, () => this.Quit(), "Back", ConsoleKey.Escape)
                        .Request();
                }
                break;

            case Stage.GameSetup:
                {
                    this.grid = Grid.GridFirst;
                    this.posPlayer = new Vector2(this.grid.Width / 2, this.grid.Height / 2);
                    this.stage = Stage.Game;
                }
                break;

            case Stage.Game:
                {
                    Console.SetCursorPosition(0, 0);
                    int consoleHeight = this.grid.Height + 12;

                    if (Program.DebugMode)
                    {
                        Util.Print();
                        // Extra spaces are added to the end to clear leftover text
                        Util.Print("Pos: {0}  ", 1, this.posPlayer);
                        consoleHeight += 2;
                    }

                    Util.SetConsoleSize(this.grid.RealWidth + 8, consoleHeight);
                    Util.Print();
                    string borderHorizontal = Util.StringOf(Adventure.CHAR_BORDER_HORIZONTAL, this.grid.Width);
                    Util.Print("{0}{1}{2}", 2, Adventure.CHAR_CORNER_A, borderHorizontal, Adventure.CHAR_CORNER_B);
                    string s;

                    for (int y = this.grid.Height - 1; y >= 0; y--)
                    {
                        s = CHAR_BORDER_VERTICAL;

                        for (int x = 0; x < this.grid.Width; x++)
                        {
                            Vector2 pos = new Vector2(x, y);
                            Tile tile = this.grid.GetTile(pos);

                            if (pos == this.posPlayer)
                            {
                                s += Adventure.CHAR_PLAYER;
                            }
                            else if (this.grid.HasCoinAt(pos))
                            {
                                s += Adventure.CHAR_COIN;
                            }
                            else
                            {
                                s += tile.Chars;
                            }
                        }

                        Util.Print(s + Adventure.CHAR_BORDER_VERTICAL, 2);
                    }

                    Util.Print("{0}{1}{2}", 2, Adventure.CHAR_CORNER_B, borderHorizontal, Adventure.CHAR_CORNER_A);
                    Util.Print();
                    Util.Print("> {0}", 3, Adventure.Message);
                    Adventure.Message = "..." + Util.StringOf(" ", this.grid.RealWidth - 3);
                    Util.Print();
                    Util.Print("Coins: {0}", 4, Adventure.Coins);
                    Util.Print();
                    Util.Print("Move) W A S D", 1);
                    InputOptionBuilder.Create()
                        .AddAction('w', () => this.MovePlayer(Direction.Up), key: ConsoleKey.NumPad8)
                        .AddAction('a', () => this.MovePlayer(Direction.Left), key: ConsoleKey.NumPad4)
                        .AddAction('s', () => this.MovePlayer(Direction.Down), key: ConsoleKey.NumPad2)
                        .AddAction('d', () => this.MovePlayer(Direction.Right), key: ConsoleKey.NumPad6)
                        .AddSpacer()
                        .AddAction(Util.NULLCHAR, () => this.stage = Stage.MainMenu, "Quit", ConsoleKey.Escape)
                        .Request();
                }
                break;
        }
    }

    private void MovePlayer(Direction direction)
    {
        Vector2 newPos = this.posPlayer + direction.ToVector2();

        if (newPos.x >= 0 && newPos.x < this.grid.Width && newPos.y >= 0 && newPos.y < this.grid.Height)
        {
            Tile tile = this.grid.GetTile(newPos);
            this.grid.CheckCoins(newPos);

            // Interact if possible
            if (tile.IsInteractable)
            {
                this.grid.Interact(newPos);
            }

            // Move into space if possible
            if (!tile.StopMovement)
            {
                this.posPlayer = newPos;
            }
        }
    }

    private sealed class Tile
    {
        public static readonly Dictionary<char, Tile> TileMap = new Dictionary<char, Tile>();

        private static Tile TILE_EMPTY = new Tile("  ");
        private static Tile TILE_WALL = new Tile("▓▓", true);
        private static Tile TILE_INTERACTABLE = new Tile("░░", true, true);

        static Tile()
        {
            Tile.TileMap.Add(' ', Tile.TILE_EMPTY);
            Tile.TileMap.Add('c', Tile.TILE_EMPTY);
            Tile.TileMap.Add('w', Tile.TILE_WALL);
            Tile.TileMap.Add('i', Tile.TILE_INTERACTABLE);
        }

        public readonly string Chars;
        public readonly bool StopMovement;
        public readonly bool IsInteractable;

        public Tile(string chars, bool stopMovement = false, bool interactable = false)
        {
            if (chars.Length != 2) { throw new ArgumentException("chars.Length != 2"); }
            this.Chars = chars;
            this.StopMovement = stopMovement;
            this.IsInteractable = interactable;
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

    private sealed class Grid
    {
        // TODO when done initializing grids, check all for any interactables that don't have added info

        public static readonly Grid GridFirst;

        static Grid()
        {
            // Grid First
            string[] sa = new string[15];
            for (int i = 0; i < sa.Length; i++) { sa[i] = Util.StringOf(" ", 15); }
            sa[1] = " wwwwwwwwwwwww ";
            sa[2] = "  w         w  ";
            sa[3] = "       i       ";
            sa[7] = "   w       w   ";
            sa[11] = "   w   c   w   ";
            sa[13] = " wwwwwwwwwwwww ";
            Grid.GridFirst = new Grid(sa);
            Grid.GridFirst.AddInteraction(new Vector2(7, 11), () => Adventure.Message = "You touched it!");
        }

        public int RealWidth { get { return this.width * 2; } }
        public int Width { get { return this.width; } }
        public int Height { get { return this.height; } }

        private readonly Dictionary<Vector2, Action> interactions = new Dictionary<Vector2, Action>();
        private readonly List<Vector2> coinList = new List<Vector2>();
        private readonly Tile[][] tileGrid;
        private readonly int width;
        private readonly int height;

        public Grid(string[] raw)
        {
            if (raw.Length > 0)
            {
                this.width = raw[0].Length;
                this.height = raw.Length;
                this.tileGrid = new Tile[this.height][];

                for (int i = 0; i < height; i++)
                {
                    string str = raw[i];

                    if (str.Length == this.width)
                    {
                        int y = this.height - i - 1;
                        this.tileGrid[y] = new Tile[this.width];
                        char[] ca = str.ToCharArray();

                        for (int x = 0; x < width; x++)
                        {
                            char c = ca[x];

                            if (c == 'c')
                            {
                                this.coinList.Add(new Vector2(x, y));
                            }

                            this.tileGrid[y][x] = (Tile)c;
                        }
                    }
                    else
                    {
                        throw new ArgumentException("All rows must be same length");
                    }
                }
            }
            else
            {
                throw new ArgumentException("Grid must have at least one row");
            }
        }

        public Tile GetTile(Vector2 pos) { return this.GetTile(pos.x, pos.y); }

        public Tile GetTile(int x, int y) { return this.tileGrid[y][x]; }

        public bool HasCoinAt(Vector2 pos) { return this.coinList.Contains(pos); }

        public void AddInteraction(Vector2 pos, Action action)
        {
            if (this.GetTile(pos).IsInteractable)
            {
                this.interactions.Add(pos, action);
            }
            else
            {
                throw new ArgumentException("Tile is not interactable");
            }
        }

        public void CheckCoins(Vector2 pos)
        {
            if (this.coinList.Contains(pos))
            {
                this.coinList.Remove(pos);
                Adventure.Coins++;
                Adventure.Message = "You picked up a coin!";
            }
        }

        public void Interact(Vector2 pos)
        {
            if (this.interactions.ContainsKey(pos))
            {
                this.interactions[pos]();
            }
            else
            {
                throw new ArgumentException(string.Format("No interaction at position {0}", pos));
            }
        }
    }

    private enum Stage
    {
        MainMenu,
        GameSetup,
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
    private readonly List<Tuple<ConsoleKey, char, string, Action>> actions = new List<Tuple<ConsoleKey, char, string, Action>>();
    private string message;

    private static string guess = "";
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
        if (this.message != null)
        {
            Util.Print();
            Util.Print("  " + this.message);
        }

        bool printLine = true;

        foreach (Tuple<ConsoleKey, char, string, Action> action in this.actions)
        {
            // If action is null, add space in display
            // If action's char or string is null, don't display option
            if (action != null)
            {
                if (action.Item3 != null)
                {
                    string s;

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

        // This needs to be here for parsing InputOptionBuilder Numbers
        int.TryParse(InputOptionBuilder.guess, out InputOptionBuilder.guessNum);

        // Get User Key Info once, otherwise, it will call different keys each loop
        ConsoleKeyInfo inputKeyInfo = Console.ReadKey(true);

        foreach (Tuple<ConsoleKey, char, string, Action> action in this.actions)
        {
            if (action != null && (action.Item1 == inputKeyInfo.Key || (action.Item2 != default(char) && action.Item2 == inputKeyInfo.KeyChar)))
            {
                action.Item4.Invoke();
                break;
            }
        }
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
        InputOptionBuilder.guess = "";
        InputOptionBuilder.guessNum = 0;
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

    public Vector2() { }

    public Vector2(int size) : this(size, size) { }

    public Vector2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override int GetHashCode() { return (this.x * this.y).GetHashCode(); }

    public override bool Equals(object obj) { return obj is Vector2 && this == (Vector2)obj; }

    public static Vector2 operator +(Vector2 vecA, Vector2 vecB) { return new Vector2(vecA.x + vecB.x, vecA.y + vecB.y); }

    public static Vector2 operator -(Vector2 vecA, Vector2 vecB) { return new Vector2(vecA.x - vecB.x, vecA.y - vecB.y); }

    public static Vector2 operator *(Vector2 vec, int i) { return new Vector2(vec.x * i, vec.y * i); }

    public static Vector2 operator /(Vector2 vec, int i) { return new Vector2(vec.x / i, vec.y / i); }

    public static bool operator ==(Vector2 vecA, Vector2 vecB) { return vecA.x == vecB.x && vecA.y == vecB.y; }

    public static bool operator !=(Vector2 vecA, Vector2 vecB) { return !(vecA == vecB); }

    public sealed override string ToString() { return string.Format("({0}, {1})", this.x, this.y); }
}

public static class Util
{
    public const char DOT = '·';
    public const char NULLCHAR = default(char);

    public static readonly Random Random = new Random();

    public static void WaitForInput() { Console.ReadKey(true); }

    public static string StringOf(string str, int repeat)
    {
        string s = "";

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
}
