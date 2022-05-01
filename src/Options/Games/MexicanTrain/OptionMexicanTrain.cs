using B.Inputs;
using B.Utils;

namespace B.Options.Games.MexicanTrain
{
    public sealed class OptionMexicanTrain : Option<OptionMexicanTrain.Stages>
    {
        #region Constants

        public const int DOMINO_MAX = 12;
        public const int PLAYERS_MIN = 2;
        public const int PLAYERS_MAX = 8;
        public const int DOMINO_START = 10; // TODO change to take into account the number of players

        #endregion



        #region Universal Properties

        public static string Title => "Mexican Train";
        // TODO implement saving/loading
        public static string FilePath => Program.DataPath + "mexicanTrain";

        #endregion



        #region Private Variables

        private MexicanTrainInfo _info = null!;

        #endregion



        #region Constructors

        public OptionMexicanTrain() : base(Stages.MainMenu) { }

        #endregion



        #region Override Methods

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.Clear();
                        Window.SetSize(34, 7);
                        Cursor.Set(0, 1);
                        Choice choice = new(OptionMexicanTrain.Title);
                        choice.AddText(new Text($"Press <{PLAYERS_MIN}-{PLAYERS_MAX}> to select players."));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateOptionExit(this));
                        for (int i = PLAYERS_MIN; i <= PLAYERS_MAX; i++)
                        {
                            // Create new instances because 'i' will change while looping
                            char c = (char)('0' + i);
                            int j = i;

                            // Add keybinds
                            choice.AddKeybind(Keybind.Create(() =>
                            {
                                _info = new MexicanTrainInfo(j);
                                _info.SetupGame();
                                SetStage(Stages.Game);
                            }, keyChar: c));
                        }
                        choice.Request();
                    }
                    break;

                case Stages.Game:
                    {
                        Window.Clear();
                        Window.SetSize(60, 20);
                        Cursor.Set(2, 1);
                        Window.Print($"Players: {_info.Players}");

                        // TODO
                        _info.HandleTurn();

                        // TODO
                    }
                    throw new NotImplementedException();
            }
        }

        #endregion



        #region Enums

        public enum Stages
        {
            MainMenu,
            Game,
        }

        #endregion
    }
}
