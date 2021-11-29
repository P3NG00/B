using System.Data;
using System.Collections.Generic;
using System.Collections;
using System;

/* ================= *\
||                   ||
||        B's        ||
||                   ||
||  Created:         ||
||     2021.11.17    ||
||                   ||
||  Edited:          ||
||     2021.11.25    ||
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
    private int guessNum;
    private string guess;

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
                    this.guessNum = 0;
                    this.guess = "";
                    this.stage = Stage.Game;
                }
                break;

            case Stage.Game:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 7);
                    Util.Print();
                    Util.Print(this.guess, 2);
                    Util.Print();

                    string guessMessage = "Between 0 - " + this.numMax;

                    if (this.guess.Length > 0)
                    {
                        this.guessNum = int.Parse(this.guess);

                        if (this.guessNum < this.number)
                        {
                            guessMessage = "too low...";
                        }
                        else if (this.guessNum > this.number)
                        {
                            guessMessage = "TOO HIGH!!!";
                        }
                    }

                    bool won = this.guessNum == this.number;

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
                        // TODO turn into static function in InputOptionBuilder
                        // ATTEMPTED: could not figure out lambdas and static and ref usage, try again later
                        InputOptionBuilder iob = InputOptionBuilder.Create("Enter a Number!", false)
                            .AddAction(default(char), () => this.guess = this.guess.Substring(0, Math.Max(0, this.guess.Length - 1)), key: ConsoleKey.Backspace);

                        for (int i = 0; i < 10; i++)
                        {
                            char c = (char)('0' + i);

                            iob.AddAction(c, () =>
                            {
                                if (int.TryParse(this.guess + c, out this.guessNum))
                                {
                                    this.guess = this.guessNum.ToString();
                                }
                            });
                        }

                        iob.Request();
                    }
                }
                break;

            case Stage.Settings:
                {
                    Console.Clear();
                    Util.SetConsoleSize(20, 10);

                    Util.Print();
                    Util.Print("Max - {0}", 1, this.numMax);

                    InputOptionBuilder.Create("Settings")
                        .AddAction('1', () =>
                        {
                            // TODO read line of int and put into numMax
                            // WANT TO DO: use some static method in InputOptionBuilder for universal number control    
                        }, "Max Number")
                        .AddSpacer()
                        .AddAction('0', () => this.stage = Stage.MainMenu, "Back")
                        .Request();
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
    }
}

public sealed class Solitaire : Option
{
    Card[] deck = new Card[Util.DECKSIZE];

    // TODO turn into game initialization function in setup stage
    private void Start()
    {
        // Create copy of deck for shuffling
        List<Card> deckTemp = new List<Card>(Util.DeckOriginal);
        int r;

        // Take all cards out of 'deckTemp' at random order
        for (int i = 0; i < Util.DECKSIZE; i++)
        {
            r = Util.Random.Next(deckTemp.Count);
            deck[i] = deckTemp[r];
            deckTemp.RemoveAt(r);
        }



        // TODO shuffle cards
        // TODO deal into piles
    }

    public sealed override void Loop()
    {
        // TODO actually do something
        Console.ReadKey();
    }
}

public sealed class InputOptionBuilder
{
    private readonly List<Tuple<ConsoleKey, char, string, Action>> actions = new List<Tuple<ConsoleKey, char, string, Action>>();
    private string message;
    private bool displayOptions;

    private InputOptionBuilder(string message, bool displayOptions)
    {
        this.message = message;
        this.displayOptions = displayOptions;
    }

    public static InputOptionBuilder Create(string message, bool displayOptions = true) { return new InputOptionBuilder(message, displayOptions); }

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
        // Add Escape key to end of list. This is overridden if the escape key finds a different bind before this one.
        this.AddAction(default(char), () => Environment.Exit(0), key: ConsoleKey.Escape);

        // Display Message
        Util.Print();
        Util.Print("  " + message);

        if (this.displayOptions)
        {
            Util.Print();

            foreach (Tuple<ConsoleKey, char, string, Action> action in this.actions)
            {
                // If action is null, add space in display
                // If action's char or string is null, don't display option
                if (action != null)
                {
                    if (action.Item2 != default(char) && action.Item3 != null)
                    {
                        Util.Print("{0}) {1}", 1, action.Item2, action.Item3);
                    }
                }
                else
                {
                    Util.Print();
                }
            }
        }

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
    public const int DECKSIZE = 52;
    public const int SUITSIZE = 13;

    public static readonly Random Random = new Random();
    public static readonly Card[] DeckOriginal = new Card[DECKSIZE];

    static Util() { for (int i = 0; i < DECKSIZE; i++) DeckOriginal[i] = new Card(new string[] { "Clubs", "Diamonds", "Hearts", "Spades" }[i / SUITSIZE], (i % SUITSIZE) + 1); }

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
}
