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
||     2021.12.03    ||
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
    public bool Running = true;

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
                        .AddAction(Util.NULLCHAR, () => this.Running = false, "Back", ConsoleKey.Escape)
                        .Request();
                }
                break;

            case Stage.GameSetup:
                {
                    this.number = Util.Random.Next(this.numMax + 1);
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
                        guessMessage = "Right On!"; // TODO create random list this pulls from ("Perfect!", "Correct!")
                    }

                    Util.Print(guessMessage, 2);

                    if (won)
                    {
                        Util.WaitForInput();
                        this.stage = Stage.MainMenu;
                    }
                    else
                    {
                        InputOptionBuilder.CreateNumbersRequest("Enter a Number!").Request();
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
                    Util.SetConsoleSize(20, 5);
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
    // TODO create stuff to do in adventure
    // TODO try new position for player and if it is a wall, don't move
    // TODO create maps that consist of walls
    // TODO create things to interact with,
    //  interact by standing next to the object and moving into it

    // Chars
    private const string CHAR_PLAYER = "()";
    private const string CHAR_ENEMY = "[]";
    private const string CHAR_EMPTY = "  ";
    private const string CHAR_WALL = "▓▓";
    private const string CHAR_BORDER_HORIZONTAL = "==";
    private const string CHAR_BORDER_VERTICAL = "||";
    private const string CHAR_CORNER_A = "//";
    private const string CHAR_CORNER_B = @"\\";

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
                        .AddAction(Util.NULLCHAR, () => this.Running = false, "Back", ConsoleKey.Escape)
                        .Request();
                }
                break;

            case Stage.GameSetup:
                {
                    this.grid = Grids.GridFirst;
                    this.posPlayer = new Vector2(this.grid.Width / 2, this.grid.Height / 2);
                    this.stage = Stage.Game;
                }
                break;

            case Stage.Game:
                {
                    Console.Clear();
                    Util.SetConsoleSize(this.grid.RealWidth + 8, this.grid.Height + 8);
                    Util.Print();
                    string borderHorizontal = Util.StringOf(CHAR_BORDER_HORIZONTAL, this.grid.Width);
                    Util.Print("{0}{1}{2}", 2, CHAR_CORNER_A, borderHorizontal, CHAR_CORNER_B);
                    string s;

                    for (int y = this.grid.Height - 1; y >= 0; y--)
                    {
                        s = CHAR_BORDER_VERTICAL;

                        for (int x = 0; x < this.grid.Width; x++)
                        {
                            Vector2 pos = new Vector2(x, y);
                            char c = this.grid.GetChar(x, y);

                            if (pos == this.posPlayer)
                            {
                                s += CHAR_PLAYER;
                            }
                            else if (c == 'w')
                            {
                                s += CHAR_WALL;
                            }
                            else
                            {
                                s += CHAR_EMPTY;
                            }
                        }

                        Util.Print(s + CHAR_BORDER_VERTICAL, 2);
                    }

                    Util.Print("{0}{1}{2}", 2, CHAR_CORNER_B, borderHorizontal, CHAR_CORNER_A);
                    Util.Print();
                    Util.Print("Move) W A S D", 1);
                    InputOptionBuilder.Create()
                        .AddAction('w', () => this.MovePlayer(Direction.Up))
                        .AddAction('a', () => this.MovePlayer(Direction.Left))
                        .AddAction('s', () => this.MovePlayer(Direction.Down))
                        .AddAction('d', () => this.MovePlayer(Direction.Right))
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

        if (newPos.x >= 0 && newPos.x < this.grid.Width && newPos.y >= 0 && newPos.y < this.grid.Height && this.grid.GetChar(newPos) != 'w')
        {
            this.posPlayer = newPos;
        }
    }

    private static class Grids
    {
        public static readonly Grid GridFirst;

        static Grids()
        {
            // Grid First
            string[] sa = new string[15];
            for (int i = 0; i < sa.Length; i++) { sa[i] = Util.StringOf(" ", 15); }
            sa[1] = " wwwwwwwwwwwww ";
            sa[7] = "   w   w   w   ";
            sa[13] = " wwwwwwwwwwwww ";
            Grids.GridFirst = new Grid(sa);
        }
    }

    private class Grid
    {
        private readonly string[] map;
        private readonly int width;
        private readonly int height;

        public int RealWidth { get { return this.width * 2; } }
        public int Width { get { return this.width; } }
        public int Height { get { return this.height; } }

        public char GetChar(Vector2 pos) { return this.GetChar(pos.x, pos.y); }

        public char GetChar(int x, int y) { return this.map[this.Height - y - 1].ToCharArray()[x]; }

        public Grid(string[] map)
        {
            if (map.Length > 0)
            {
                int length = map[0].Length;

                for (int i = 1; i < map.Length; i++)
                {
                    if (map[i].Length != length)
                    {
                        throw new ArgumentException("All rows must be the same length");
                    }
                }

                this.map = map;
                this.width = map[0].Length;
                this.height = map.Length;
            }
            else
            {
                throw new ArgumentException("Map must have at least one row");
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

        // TODO figure out if this needs to be here?
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
