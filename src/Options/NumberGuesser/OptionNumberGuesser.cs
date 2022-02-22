using B.Inputs;
using B.Utils;

namespace B.Options.NumberGuesser
{
    public sealed class OptionNumberGuesser : Option<OptionNumberGuesser.Stages>
    {
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
                        new Input.Choice("Number Guesser")
                            .Add(() =>
                            {
                                this._numRandom = Util.Random.Next(this._numMax) + 1;
                                Input.String = string.Empty;
                                this.Stage = Stages.Game;
                            }, "New Game", '1')
                            .AddSpacer()
                            .Add(() => this.Stage = Stages.Settings, "Settings", '9')
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

                        if (Program.Settings.DebugMode.Active)
                        {
                            Window.PrintLine();
                            Window.PrintLine($" Number: {this._numRandom}");
                            consoleHeight += 2;
                        }

                        Window.ClearAndSetSize(20, consoleHeight);
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
                            this.Stage = Stages.MainMenu;
                        }
                        else
                        {
                            Window.PrintLine();
                            Window.PrintLine(" Enter a Number!");

                            if (Input.RequestLine(OptionNumberGuesser.GUESS_LENGTH).Key == ConsoleKey.Escape)
                                this.Stage = Stages.MainMenu;
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
                                this.Stage = Stages.Settings_MaxNumber;
                            }, "Max Number", '1')
                            .AddSpacer()
                            .Add(() => this.Stage = Stages.MainMenu, "Back", key: ConsoleKey.Escape)
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
                        ConsoleKey key = Input.RequestLine(OptionNumberGuesser.GUESS_LENGTH).Key;

                        if (key == ConsoleKey.Enter)
                        {
                            int? numMax = Input.Int;

                            if (numMax.HasValue)
                            {
                                this._numMax = Math.Max(1, numMax.Value);
                                this.Stage = Stages.Settings;
                            }
                        }
                        else if (key == ConsoleKey.Escape)
                            this.Stage = Stages.Settings;
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
