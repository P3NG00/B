using B.Inputs;
using B.Utils;

namespace B.Options.NumberGuesser
{
    public sealed class OptionNumberGuesser : Option
    {
        private static readonly string[] _winMessages = new string[]
        {
            "Right on!",
            "Perfect!",
            "Correct!",
        };

        private Stage _stage = Stage.MainMenu;
        private int _numMax = 100;
        private int _guessNum;

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
                                this._guessNum = Util.Random.Next(this._numMax) + 1;
                                Input.Int = 0;
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
                        bool won = this._guessNum == Input.Int;
                        int consoleHeight = 7;

                        if (Program.DebugMode)
                        {
                            Util.Print(string.Format("Number: {0,-3}", this._guessNum), 1, linesBefore: 1);
                            consoleHeight += 2;
                        }

                        Util.SetConsoleSize(20, consoleHeight);
                        Util.Print(Input.Int, 2, linesBefore: 1);
                        guessMessage = Input.Int.ToString().Length == 0 ? "..." :
                            won ? OptionNumberGuesser._winMessages[Util.Random.Next(OptionNumberGuesser._winMessages.Length)] :
                                Input.Int < this._guessNum ? "too low..." : "TOO HIGH!!!";

                        Util.Print(guessMessage, 2, linesBefore: 1);

                        if (won)
                        {
                            Util.WaitForInput();
                            this._stage = Stage.MainMenu;
                        }
                        else
                        {
                            Util.Print("Enter a Number!", 1, linesBefore: 1);

                            if (Input.RequestInt() == ConsoleKey.Escape)
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
                                Input.Int = this._numMax;
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
                        Util.Print(string.Format("Max - {0}", Input.Int), 2, linesBefore: 1);
                        Util.Print("Enter Max Number", 2, linesBefore: 1);
                        ConsoleKey key = Input.RequestInt();

                        if (key == ConsoleKey.Enter)
                        {
                            if (Input.Int < 1)
                                Input.Int = 1;

                            this._numMax = Input.Int;
                            this._stage = Stage.Settings;
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
