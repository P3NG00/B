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

        // TODO add save/load settings. Keep last "Max Guess Num" and "Use Decimals"

        private int _numMax = 100;
        private int _numRandom;

        public OptionNumberGuesser() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            switch (this.Stage)
            {
                case Stages.MainMenu:
                    {
                        Util.ClearConsole(20, 8);
                        new Input.Option("Number Guesser")
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

                        if (Program.Settings.DebugMode)
                        {
                            Util.PrintLine();
                            Util.PrintLine($" Number: {this._numRandom}");
                            consoleHeight += 2;
                        }

                        Util.ClearConsole(20, consoleHeight);
                        Util.PrintLine();
                        Util.PrintLine($"  {Input.String}");

                        if (guess == null)
                            guessMessage = "...";
                        else if (guess < this._numRandom)
                            guessMessage = "too low...";
                        else if (guess > this._numRandom)
                            guessMessage = "TOO HIGH!!!";
                        else
                            guessMessage = OptionNumberGuesser._winMessages[Util.Random.Next(0, OptionNumberGuesser._winMessages.Length)];

                        Util.PrintLine();
                        Util.PrintLine($"  {guessMessage}");

                        if (won)
                        {
                            Util.GetKey();
                            this.Stage = Stages.MainMenu;
                        }
                        else
                        {
                            Util.PrintLine();
                            Util.PrintLine(" Enter a Number!");

                            if (Input.RequestLine(OptionNumberGuesser.GUESS_LENGTH) == ConsoleKey.Escape)
                                this.Stage = Stages.MainMenu;
                        }
                    }
                    break;

                case Stages.Settings:
                    {
                        Util.ClearConsole(20, 7);
                        new Input.Option("Settings")
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
                        Util.ClearConsole(20, 5);
                        Util.PrintLine();
                        Util.PrintLine($"  Max - {Input.String}");
                        Util.PrintLine();
                        Util.PrintLine("  Enter Max Number");
                        ConsoleKey key = Input.RequestLine(OptionNumberGuesser.GUESS_LENGTH);

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
