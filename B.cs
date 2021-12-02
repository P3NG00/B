using System.Net;
using System.Resources;
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
||     2021.12.01    ||
||                   ||
\* ================= */

/*

TODO options
Solitaire
Minesweeper
Blackjack
Adventure (walking around grid)

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
                    .AddAction('0', () => this.running = false, "Quit")
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

        // TODO exit code, save data
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
                        .AddAction('0', () => this.Running = false, "Back")
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
                    string guess = InputOptionBuilder.GetGuess();
                    int guessNum = InputOptionBuilder.GetGuessNum();
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
                        .AddAction('0', () => this.stage = Stage.MainMenu, "Back")
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
                        .AddAction(default(char), () => this.stage = Stage.Settings, key: ConsoleKey.Escape)
                        .Request();
                    this.numMax = InputOptionBuilder.GetGuessNum();
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
    private const string CHARPLAYER = "()";
    private const string CHARENEMY = "[]";
    private const string CHAREMPTY = "  ";

    private const string CHARCORNER0 = "//";
    private const string CHARCORNER1 = "\\\\";

    // The 'square' size of the grid
    // TODO create maps of different sizes and keep this info in that class
    private int displayWidth = 11;
    private int displayHeight = 11;

    private Stage stage = Stage.MainMenu;

    private Vector2 posPlayer;

    public sealed override void Loop()
    {
        switch (this.stage)
        {
            case Stage.MainMenu:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 7); // TODO adjust size appropriately
                    InputOptionBuilder.Create("Adventure")
                        .AddAction('1', () => this.stage = Stage.GameSetup, "New Game")
                        .AddSpacer()
                        .AddAction('0', () => this.Running = false, "Back")
                        .Request();
                }
                break;

            // TODO remove this if the only line is switching the stage to game
            case Stage.GameSetup:
                {
                    this.posPlayer = new Vector2(this.displayWidth / 2, this.displayHeight / 2);
                    this.stage = Stage.Game;
                }
                break;

            case Stage.Game:
                {
                    Console.Clear();
                    Util.SetConsoleSize((this.displayWidth * 2) + 8, this.displayHeight + 8); // TODO figure out if num additions are necessary
                    Util.Print();
                    string borderHorizontal = Util.StringOf("=", this.displayWidth * 2);
                    Util.Print("{0}{1}{2}", 2, CHARCORNER0, borderHorizontal, CHARCORNER1);
                    string s;

                    for (int y = displayHeight - 1; y >= 0; y--)
                    {
                        s = "||";

                        for (int x = 0; x < this.displayWidth; x++)
                        {
                            Vector2 pos = new Vector2(x, y);

                            if (pos == this.posPlayer)
                            {
                                s += CHARPLAYER;
                            }
                            else
                            {
                                s += CHAREMPTY;
                            }
                        }

                        Util.Print(s + "||", 2);
                    }

                    Util.Print("{0}{1}{2}", 2, CHARCORNER1, borderHorizontal, CHARCORNER0);
                    Util.Print();
                    Util.Print("Move) WASD", 1);

                    // TODO DONT LET PLAYER MOVE OUTSIDE OF AREA
                    InputOptionBuilder.Create()
                        .AddAction('w', () => this.posPlayer.y++)
                        .AddAction('a', () => this.posPlayer.x--)
                        .AddAction('s', () => this.posPlayer.y--)
                        .AddAction('d', () => this.posPlayer.x++)
                        .AddSpacer()
                        .AddAction(default(char), () => this.stage = Stage.MainMenu, "Quit", ConsoleKey.Escape)
                        .Request();
                }
                break;
        }
    }

    private enum Stage
    {
        MainMenu,
        GameSetup,
        Game,
    }
}

public sealed class InputOptionBuilder
{
    private readonly List<Tuple<ConsoleKey, char, string, Action>> actions = new List<Tuple<ConsoleKey, char, string, Action>>();
    private string message;

    private static string guess = "";
    private static int guessNum = 0;

    public static string GetGuess() { return InputOptionBuilder.guess; }
    public static int GetGuessNum() { return InputOptionBuilder.guessNum; }

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
                // TODO print key if Description exists but Char does not, use ConsoleKey as char
                if (action.Item3 != null)
                {
                    string s;

                    if (action.Item2 == default(char))
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
        ConsoleKeyInfo inputKeyInfo = Util.GetUserKeyInfo();

        foreach (Tuple<ConsoleKey, char, string, Action> action in this.actions)
        {
            if (action != null && (action.Item1 == inputKeyInfo.Key || action.Item2 == inputKeyInfo.KeyChar))
            {
                action.Item4.Invoke();
                break;
            }
        }
    }

    public static InputOptionBuilder CreateNumbersRequest(string message)
    {
        InputOptionBuilder iob = InputOptionBuilder.Create(message)
            .AddAction(default(char), () =>
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
    public int x = 0;
    public int y = 0;

    public Vector2() { }

    public Vector2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override int GetHashCode() { return (this.x * this.y).GetHashCode(); }

    public override bool Equals(object obj) { return obj is Vector2 && this == (Vector2)obj; }

    public static bool operator ==(Vector2 vec0, Vector2 vec1) { return vec0.x == vec1.x && vec0.y == vec1.y; }

    public static bool operator !=(Vector2 vec0, Vector2 vec1) { return !(vec0 == vec1); }
}

public static class Util
{
    public const char DOT = '·';

    public static readonly Random Random = new Random();

    // TODO if only referenced once, inline function
    public static ConsoleKeyInfo GetUserKeyInfo() { return Console.ReadKey(true); }

    // TODO if only referenced once, inline function
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

    // TODO if only referenced once, inline function
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

    // TODO if only referenced once, inline function
    //
    // in every place this functions is called, Console.Clear() is also called, if this is true later, add that to this function and cleanup places it is used
    // or
    // create a thread that keeps the console size and constantly updates it
    public static void SetConsoleSize(int width, int height)
    {
        Console.SetWindowSize(width, height);
        Console.SetBufferSize(Console.WindowLeft + width, Console.WindowTop + height);
        Console.SetWindowSize(width, height);
    }

    // TODO if only referenced once, inline function
    public static void ToggleBool(ref bool b) { b = !b; }

    // TODO if not referenced, remove function
    public static void Quit() { Environment.Exit(0); }
}
