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
||     2021.11.29    ||
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
    public const bool DEBUG = true;

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
                    .AddAction('2', () => this.option = new Solitaire(), "Solitaire")
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
                Util.Print(e.StackTrace);
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

    enum Stage
    {
        MainMenu,
        GameSetup,
        Game,
        Settings,
        Settings_MaxNumber,
    }
}

public sealed class Solitaire : Option
{
    private Stage stage = Stage.MainMenu;

    public sealed override void Loop()
    {
        switch (this.stage)
        {
            case Stage.MainMenu:
                {
                }
                break;

            case Stage.GameSetup:
                {
                }
                break;

            case Stage.Game:
                {
                }
                break;
        }
    }

    enum Stage
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

    public static InputOptionBuilder Create(string message) { return new InputOptionBuilder(message); }

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
        // Display Message
        Util.Print();
        Util.Print("  " + message);
        bool printLine = true;

        foreach (Tuple<ConsoleKey, char, string, Action> action in this.actions)
        {
            // If action is null, add space in display
            // If action's char or string is null, don't display option
            if (action != null)
            {
                if (action.Item2 != default(char) && action.Item3 != null)
                {
                    if (printLine)
                    {
                        printLine = false;
                        Util.Print();
                    }

                    Util.Print("{0}) {1}", 1, action.Item2, action.Item3);
                }
            }
            else if (!printLine)
            {
                printLine = true;
            }
        }

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

public sealed class Deck
{
    public const int DECKSIZE = 52;
    public const int SUITSIZE = 13;

    private readonly List<Card> cards = new List<Card>();

    public void DealTo(Deck deck)
    {
        if (this.cards.Count > 0)
        {
            deck.cards.Add(this.cards[0]);
            this.cards.RemoveAt(0);
        }
    }

    public static Deck CreateShuffledDeck()
    {
        string[] suits = new String[] { "Clubs", "Diamonds", "Hearts", "Spades" };
        Deck deckTemp = new Deck();
        Deck deck = new Deck();

        for (int i = 0; i < DECKSIZE; i++)
        {
            deckTemp.cards.Add(new Card(suits[i / SUITSIZE], (i % SUITSIZE) + 1));
        }

        // Take all cards out of 'deckTemp' at random order
        for (int i = 0; i < DECKSIZE; i++)
        {
            int r = Util.Random.Next(deckTemp.cards.Count);
            deck.cards.Add(deckTemp.cards[r]);
            deckTemp.cards.RemoveAt(r);
        }

        return deck;
    }
}

public sealed class Card
{
    private readonly string suit;
    private readonly int value;

    public Card(string suit, int value)
    {
        this.suit = suit;
        this.value = value;
    }

    private string GetValueIcon()
    {
        switch (this.value)
        {
            case 11: return " J";
            case 12: return " Q";
            case 13: return " K";
            default: return string.Format("{0,2}", value);
        }
    }

    public override string ToString() { return " " + this.GetValueIcon() + " of " + this.suit; }
}

public static class Util
{
    public static readonly Random Random = new Random();

    // TODO if only referenced once, inline function
    public static ConsoleKeyInfo GetUserKeyInfo() { return Console.ReadKey(true); }

    // TODO if only referenced once, inline function
    public static void WaitForInput() { Console.ReadKey(true); }

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
    public static void SetConsoleSize(int width, int height)
    {
        Console.SetWindowSize(width, height);
        Console.SetBufferSize(Console.WindowLeft + width, Console.WindowTop + height);
        Console.SetWindowSize(width, height);
    }

    // TODO if only referenced once, inline function
    public static void ToggleBool(ref bool b) { b = !b; }

    public static void Quit() { Environment.Exit(0); }
}
