using B.Inputs;
using B.Utils;

namespace B.Options.NumberGuesser
{
    public sealed class OptionNumberGuesser : Option
    {
        private const int GUESS_LENGTH = 9;

        private static readonly string[] _winMessages = new string[]
        {
            "Right on!",
            "Perfect!",
            "Correct!",
            "Nice one!",
        };

        private Stage _stage = Stage.MainMenu;
        private int _numMax = 100;
        private int _numRandom;

        public sealed override void Loop()
        {
            switch (this._stage)
            {
                case Stage.MainMenu:
                    {
                        Util.ClearConsole(20, 8);
                        new Input.Option("Number Guesser")
                            .Add(() =>
                            {
                                this._numRandom = Util.Random.Next(this._numMax) + 1;
                                Input.String = string.Empty;
                                this._stage = Stage.Game;
                            }, "New Game", '1')
                            .AddSpacer()
                            .Add(() => this._stage = Stage.Settings, "Settings", '9')
                            .Add(() => this.Quit(), "Exit", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stage.Game:
                    {
                        string guessMessage = "Between 0 - " + this._numMax;
                        int? guess = Input.Int;
                        bool won = guess.HasValue && guess.Value == this._numRandom;
                        int consoleHeight = 5;

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
                            Util.WaitForInput();
                            this._stage = Stage.MainMenu;
                        }
                        else
                        {
                            Util.PrintLine();
                            Util.PrintLine(" Enter a Number!");

                            if (Input.Request(OptionNumberGuesser.GUESS_LENGTH) == ConsoleKey.Escape)
                                this._stage = Stage.MainMenu;
                        }
                    }
                    break;

                case Stage.Settings:
                    {
                        Util.ClearConsole(20, 7);
                        new Input.Option("Settings")
                            .Add(() =>
                            {
                                Input.String = this._numMax.ToString();
                                this._stage = Stage.Settings_MaxNumber;
                            }, "Max Number", '1')
                            .AddSpacer()
                            .Add(() => this._stage = Stage.MainMenu, "Back", key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stage.Settings_MaxNumber:
                    {
                        Util.ClearConsole(20, 5);
                        Util.PrintLine();
                        Util.PrintLine($"  Max - {Input.String}");
                        Util.PrintLine();
                        Util.PrintLine("  Enter Max Number");
                        ConsoleKey key = Input.Request(OptionNumberGuesser.GUESS_LENGTH);

                        if (key == ConsoleKey.Enter)
                        {
                            int? numMax = Input.Int;

                            if (numMax.HasValue)
                            {
                                this._numMax = Math.Max(1, numMax.Value);
                                this._stage = Stage.Settings;
                            }
                        }
                        else if (key == ConsoleKey.Escape)
                            this._stage = Stage.Settings;
                    }
                    break;
            }
        }

        private enum Stage
        {
            MainMenu,
            Game,
            Settings,
            Settings_MaxNumber,
        }
    }
}
