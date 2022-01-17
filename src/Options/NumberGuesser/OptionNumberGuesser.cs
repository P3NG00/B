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
        };

        private Stage _stage = Stage.MainMenu;
        private int _numMax = 100;
        private int _numRandom;

        public sealed override void Loop()
        {
            Console.Clear();

            switch (this._stage)
            {
                case Stage.MainMenu:
                    {
                        Util.SetConsoleSize(20, 8);
                        new Input.Option("Number Guesser")
                            .AddKeybind(new Keybind(() =>
                            {
                                this._numRandom = Util.Random.Next(this._numMax) + 1;
                                Input.String = string.Empty;
                                this._stage = Stage.Game;
                            }, "New Game", '1'))
                            .AddSpacer()
                            .AddKeybind(new Keybind(() => this._stage = Stage.Settings, "Settings", '9'))
                            .AddKeybind(new Keybind(() => this.Quit(), "Back", key: ConsoleKey.Escape))
                            .Request();
                    }
                    break;

                case Stage.Game:
                    {
                        string guessMessage = "Between 0 - " + this._numMax;
                        int? guess = Input.Int;
                        bool won = guess.HasValue && guess.Value == this._numRandom;
                        int consoleHeight = 7;

                        if (Program.Settings.DebugMode)
                        {
                            Util.Print($"Number: {this._numRandom}", 1, linesBefore: 1);
                            consoleHeight += 2;
                        }

                        Util.SetConsoleSize(20, consoleHeight);
                        Util.Print(Input.String, 2, linesBefore: 1);

                        if (guess == null)
                            guessMessage = "...";
                        else if (guess < this._numRandom)
                            guessMessage = "too low...";
                        else if (guess > this._numRandom)
                            guessMessage = "TOO HIGH!!!";
                        else
                            guessMessage = _winMessages[Util.Random.Next(0, _winMessages.Length)];

                        Util.Print(guessMessage, 2, linesBefore: 1);

                        if (won)
                        {
                            Util.WaitForInput();
                            this._stage = Stage.MainMenu;
                        }
                        else
                        {
                            Util.Print("Enter a Number!", 1, linesBefore: 1);

                            if (Input.Request(OptionNumberGuesser.GUESS_LENGTH) == ConsoleKey.Escape)
                                this._stage = Stage.MainMenu;
                        }
                    }
                    break;

                case Stage.Settings:
                    {
                        Util.SetConsoleSize(20, 7);
                        new Input.Option("Settings")
                            .AddKeybind(new Keybind(() =>
                            {
                                Input.String = this._numMax.ToString();
                                this._stage = Stage.Settings_MaxNumber;
                            }, "Max Number", '1'))
                            .AddSpacer()
                            .AddKeybind(new Keybind(() => this._stage = Stage.MainMenu, "Back", key: ConsoleKey.Escape))
                            .Request();
                    }
                    break;

                case Stage.Settings_MaxNumber:
                    {
                        Util.SetConsoleSize(20, 5);
                        Util.Print($"Max - {Input.String}", 2, linesBefore: 1);
                        Util.Print("Enter Max Number", 2, linesBefore: 1);
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
