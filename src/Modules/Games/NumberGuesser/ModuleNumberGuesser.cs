using B.Inputs;
using B.Utils;
using B.Utils.Extensions;

namespace B.Modules.Games.NumberGuesser
{
    public sealed class ModuleNumberGuesser : Module<ModuleNumberGuesser.Stages>
    {
        #region Constants

        private const int GUESS_LENGTH = 9;

        #endregion



        #region Universal Properties

        public static string Title => "Number Guesser";
        public static string DirectoryPath => Program.DataPath + @"numberGuesser\";
        public static string FilePath => DirectoryPath + "last";

        #endregion



        #region Private Variables

        private static readonly string[] _winMessages = new string[]
        {
            "Right on!",
            "Perfect!",
            "Correct!",
            "Nice one!",
        };

        private int _numMax = 100;
        private int _numRandom;

        #endregion



        #region Constructors

        public ModuleNumberGuesser() : base(Stages.MainMenu)
        {
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);
            else if (File.Exists(FilePath))
                _numMax = Data.Deserialize<int>(FilePath);
        }

        #endregion



        #region Override Methods

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.SetSize(20, 8);
                        Cursor.Set(0, 1);
                        Choice choice = new(Title);
                        choice.AddKeybind(Keybind.Create(() =>
                        {
                            _numRandom = Util.Random.Next(_numMax) + 1;
                            Input.ResetString();
                            SetStage(Stages.Game);
                        }, "New Game", '1'));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.Settings), "Settings", '9'));
                        choice.AddKeybind(Keybind.CreateModuleExit(this));
                        choice.Request();
                    }
                    break;

                case Stages.Game:
                    {
                        string guessMessage = "Between 0 - " + _numMax;
                        int? guess = Input.Int;
                        bool won = guess.HasValue && guess.Value == _numRandom;
                        int consoleHeight = 7;
                        bool debug = Program.Settings.DebugMode;

                        if (debug)
                            consoleHeight += 2;

                        Window.SetSize(20, consoleHeight);
                        Cursor.Set(2, 1);

                        if (debug)
                        {
                            Cursor.Set(1, 1);
                            Window.Print($"Number: {_numRandom}");
                            Cursor.Set(2, 3);
                        }

                        Window.Print(Input.String);

                        if (guess == null)
                            guessMessage = "...";
                        else if (guess < _numRandom)
                            guessMessage = "too low...";
                        else if (guess > _numRandom)
                            guessMessage = "TOO HIGH!!!";
                        else
                            guessMessage = _winMessages.Random();

                        Cursor.NextLine(2, 2);
                        Window.Print(guessMessage);

                        if (won)
                        {
                            Input.Get();
                            SetStage(Stages.MainMenu);
                        }
                        else
                        {
                            Cursor.NextLine(2, 2);
                            Window.Print("Enter a Number!");
                            Input.RequestLine(GUESS_LENGTH, Keybind.Create(() => SetStage(Stages.MainMenu), key: ConsoleKey.Escape));
                        }
                    }
                    break;

                case Stages.Settings:
                    {
                        Window.SetSize(20, 7);
                        Cursor.Set(0, 1);
                        Choice choice = new("Settings");
                        choice.AddKeybind(Keybind.Create(() =>
                        {
                            Input.String = _numMax.ToString();
                            SetStage(Stages.Settings_MaxNumber);
                        }, "Max Number", '1'));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.Create(() => SetStage(Stages.MainMenu), "Back", key: ConsoleKey.Escape));
                        choice.Request();
                    }
                    break;

                case Stages.Settings_MaxNumber:
                    {
                        Window.SetSize(20, 5);
                        Cursor.Set(2, 1);
                        Window.Print($"Max - {Input.String}");
                        Cursor.Set(2, 3);
                        Window.Print("Enter Max Number");
                        Input.RequestLine(GUESS_LENGTH,
                            Keybind.Create(() =>
                            {
                                int? numMax = Input.Int;

                                if (numMax.HasValue)
                                {
                                    _numMax = Math.Max(1, numMax.Value);
                                    SetStage(Stages.Settings);
                                }
                            }, key: ConsoleKey.Enter),
                            Keybind.Create(() => SetStage(Stages.Settings), key: ConsoleKey.Escape)
                        );
                    }
                    break;
            }
        }

        public sealed override void Save() => Data.Serialize(FilePath, _numMax);

        public sealed override void Quit()
        {
            Save();
            base.Quit();
        }

        #endregion



        #region Enums

        public enum Stages
        {
            MainMenu,
            Game,
            Settings,
            Settings_MaxNumber,
        }

        #endregion
    }
}
