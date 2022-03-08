using B.Inputs;
using B.Utils;

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
                        Window.ClearAndSetSize(20, 8);
                        Input.CreateChoice(OptionNumberGuesser.Title)
                            .Add(() =>
                            {
                                _numRandom = Util.Random.Next(_numMax) + 1;
                                Input.ResetString(); ;
                                SetStage(Stages.Game);
                            }, "New Game", '1')
                            .AddSpacer()
                            .Add(() => SetStage(Stages.Settings), "Settings", '9')
                            .AddExit(this, false)
                            .Request();
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

                        Window.ClearAndSetSize(20, consoleHeight);

                        if (debug)
                        {
                            Window.PrintLine();
                            Window.PrintLine($" Number: {_numRandom}");
                        }

                        Window.PrintLine();
                        Window.PrintLine($"  {Input.String}");

                        if (guess == null)
                            guessMessage = "...";
                        else if (guess < _numRandom)
                            guessMessage = "too low...";
                        else if (guess > _numRandom)
                            guessMessage = "TOO HIGH!!!";
                        else
                            guessMessage = OptionNumberGuesser._winMessages.Random();

                        Window.PrintLine();
                        Window.PrintLine($"  {guessMessage}");

                        if (won)
                        {
                            Input.Get();
                            SetStage(Stages.MainMenu);
                        }
                        else
                        {
                            Window.PrintLine();
                            Window.PrintLine(" Enter a Number!");
                            Input.RequestLine(OptionNumberGuesser.GUESS_LENGTH, new Keybind(() => SetStage(Stages.MainMenu), key: ConsoleKey.Escape));
                        }
                    }
                    break;

                case Stages.Settings:
                    {
                        Window.ClearAndSetSize(20, 7);
                        Input.CreateChoice("Settings")
                            .Add(() =>
                            {
                                Input.String = _numMax.ToString();
                                SetStage(Stages.Settings_MaxNumber);
                            }, "Max Number", '1')
                            .AddSpacer()
                            .Add(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stages.Settings_MaxNumber:
                    {
                        Window.ClearAndSetSize(20, 5);
                        Window.PrintLine();
                        Window.PrintLine($"  Max - {Input.String}");
                        Window.PrintLine();
                        Window.PrintLine("  Enter Max Number");
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
