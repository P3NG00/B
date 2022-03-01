using B.Inputs;
using B.Utils;

namespace B.Options.NumberGuesser
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
            switch (this.Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.ClearAndSetSize(20, 8);
                        new Input.Choice(OptionNumberGuesser.Title)
                            .Add(() =>
                            {
                                this._numRandom = Util.Random.Next(this._numMax) + 1;
                                Input.ResetString(); ;
                                this.SetStage(Stages.Game);
                            }, "New Game", '1')
                            .AddSpacer()
                            .Add(() => this.SetStage(Stages.Settings), "Settings", '9')
                            .AddExit(this, false)
                            .Request();
                    }
                    break;

                case Stages.Game:
                    {
                        string guessMessage = "Between 0 - " + this._numMax;
                        int? guess = Input.Int;
                        bool won = guess.HasValue && guess.Value == this._numRandom;
                        int consoleHeight = 7;
                        bool debug = Program.Settings.DebugMode.Active;

                        if (debug)
                            consoleHeight += 2;

                        Window.ClearAndSetSize(20, consoleHeight);

                        if (debug)
                        {
                            Window.PrintLine();
                            Window.PrintLine($" Number: {this._numRandom}");
                        }

                        Window.PrintLine();
                        Window.PrintLine($"  {Input.String}");

                        if (guess == null)
                            guessMessage = "...";
                        else if (guess < this._numRandom)
                            guessMessage = "too low...";
                        else if (guess > this._numRandom)
                            guessMessage = "TOO HIGH!!!";
                        else
                            guessMessage = Util.RandomFrom(OptionNumberGuesser._winMessages);

                        Window.PrintLine();
                        Window.PrintLine($"  {guessMessage}");

                        if (won)
                        {
                            Input.Get();
                            this.SetStage(Stages.MainMenu);
                        }
                        else
                        {
                            Window.PrintLine();
                            Window.PrintLine(" Enter a Number!");
                            Input.RequestLine(OptionNumberGuesser.GUESS_LENGTH, new Keybind(() => this.SetStage(Stages.MainMenu), key: ConsoleKey.Escape));
                        }
                    }
                    break;

                case Stages.Settings:
                    {
                        Window.ClearAndSetSize(20, 7);
                        new Input.Choice("Settings")
                            .Add(() =>
                            {
                                Input.String = this._numMax.ToString();
                                this.SetStage(Stages.Settings_MaxNumber);
                            }, "Max Number", '1')
                            .AddSpacer()
                            .Add(() => this.SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape)
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
                                    this._numMax = Math.Max(1, numMax.Value);
                                    this.SetStage(Stages.Settings);
                                }
                            }, key: ConsoleKey.Enter),
                            new Keybind(() => this.SetStage(Stages.Settings), key: ConsoleKey.Escape));
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
