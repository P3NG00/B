using B.Inputs;
using B.Utils;
using B.Utils.Extensions;

namespace B.Options.Games.NumberGuesser
{
    public sealed class OptionNumberGuesser : Option<OptionNumberGuesser.Stages>
    {
        public const string Title = "Number Guesser";

        private const int GUESS_LENGTH = 9;

        private static readonly string[] _winMessages = new string[]
        {
            "Right on!",
            "Perfect!",
            "Correct!",
            "Nice one!",
        };

        private int _numMax = 100;
        private int _numRandom;

        public OptionNumberGuesser() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.Clear();
                        Window.SetSize(20, 8);
                        Cursor.Position = new(0, 1);
                        Input.Choice choice = Input.Choice.Create(OptionNumberGuesser.Title);
                        choice.Add(() =>
                        {
                            _numRandom = Util.Random.Next(_numMax) + 1;
                            Input.ResetString();
                            SetStage(Stages.Game);
                        }, "New Game", '1');
                        choice.AddSpacer();
                        choice.Add(() => SetStage(Stages.Settings), "Settings", '9');
                        choice.AddExit(this);
                        choice.Request();
                    }
                    break;

                case Stages.Game:
                    {
                        string guessMessage = "Between 0 - " + _numMax;
                        int? guess = Input.Int;
                        bool won = guess.HasValue && guess.Value == _numRandom;
                        int consoleHeight = 7;
                        bool debug = Program.Settings.DebugMode.Active;

                        if (debug)
                            consoleHeight += 2;

                        Window.Clear();
                        Window.SetSize(20, consoleHeight);
                        Cursor.Position = new(2, 1);

                        if (debug)
                        {
                            Cursor.Position = new(1, 1);
                            Window.Print($"Number: {_numRandom}");
                            Cursor.Position = new(2, 3);
                        }

                        Window.Print(Input.String);

                        if (guess == null)
                            guessMessage = "...";
                        else if (guess < _numRandom)
                            guessMessage = "too low...";
                        else if (guess > _numRandom)
                            guessMessage = "TOO HIGH!!!";
                        else
                            guessMessage = OptionNumberGuesser._winMessages.Random();

                        Cursor.x = 2;
                        Cursor.y += 2;
                        Window.Print(guessMessage);

                        if (won)
                        {
                            Input.Get();
                            SetStage(Stages.MainMenu);
                        }
                        else
                        {
                            Cursor.x = 1;
                            Cursor.y += 2;
                            Window.Print("Enter a Number!");
                            Input.RequestLine(OptionNumberGuesser.GUESS_LENGTH, new Keybind(() => SetStage(Stages.MainMenu), key: ConsoleKey.Escape));
                        }
                    }
                    break;

                case Stages.Settings:
                    {
                        Window.Clear();
                        Window.SetSize(20, 7);
                        Cursor.Position = new(0, 1);
                        Input.Choice choice = Input.Choice.Create("Settings");
                        choice.Add(() =>
                        {
                            Input.String = _numMax.ToString();
                            SetStage(Stages.Settings_MaxNumber);
                        }, "Max Number", '1');
                        choice.AddSpacer();
                        choice.Add(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape);
                        choice.Request();
                    }
                    break;

                case Stages.Settings_MaxNumber:
                    {
                        Window.Clear();
                        Window.SetSize(20, 5);
                        Cursor.Position = new(2, 1);
                        Window.Print($"Max - {Input.String}");
                        Cursor.Position = new(2, 3);
                        Window.Print("Enter Max Number");
                        Input.RequestLine(OptionNumberGuesser.GUESS_LENGTH,
                            new Keybind(() =>
                            {
                                int? numMax = Input.Int;

                                if (numMax.HasValue)
                                {
                                    _numMax = Math.Max(1, numMax.Value);
                                    SetStage(Stages.Settings);
                                }
                            }, key: ConsoleKey.Enter),
                            new Keybind(() => SetStage(Stages.Settings), key: ConsoleKey.Escape));
                    }
                    break;
            }
        }

        public enum Stages
        {
            MainMenu,
            Game,
            Settings,
            Settings_MaxNumber,
        }
    }
}
