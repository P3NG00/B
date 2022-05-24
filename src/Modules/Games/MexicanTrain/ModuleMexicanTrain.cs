using B.Inputs;
using B.Utils;

namespace B.Modules.Games.MexicanTrain
{
    public sealed class ModuleMexicanTrain : Module<ModuleMexicanTrain.Stages>
    {
        #region Constants

        // Max amount of dots on a domino.
        public const int DOMINO_MAX = 12;
        // Minimum amount of players.
        public const int PLAYERS_MIN = 2;
        // Maximum amount of players.
        public const int PLAYERS_MAX = 8;
        // Amount of dominoes that each player starts with. (// TODO change to take into account the number of players)
        public const int DOMINO_START = 10;

        #endregion



        #region Universal Properties

        // Module Title.
        public static string Title => "Mexican Train";
        // TODO add relative directory instead of saving directly to DataPath
        // Relative file path where data is saved. (// TODO implement saving/loading)
        public static string FilePath => Program.DataPath + "mexicanTrain";

        #endregion



        #region Private Variables

        // Mexican Train Game Info.
        private MexicanTrainInfo _info = null!;

        #endregion



        #region Constructors

        // Creates a new instance of ModuleMexicanTrain.
        public ModuleMexicanTrain() : base(Stages.MainMenu) { }

        #endregion



        #region Override Methods

        // Module Loop.
        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.SetSize(34, 7);
                        Cursor.Set(0, 1);
                        Choice choice = new(ModuleMexicanTrain.Title);
                        choice.AddText(new Text($"Press <{PLAYERS_MIN}-{PLAYERS_MAX}> to select players."));
                        choice.AddSpacer();
                        choice.AddKeybind(Keybind.CreateModuleExit(this));
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
                        Window.SetSize(60, 20);
                        Cursor.Set(2, 1);
                        Window.Print($"Players: {_info.Players}");

                        // TODO
                        _info.HandleTurn();
                    }
                    throw new NotImplementedException();
            }
        }

        #endregion



        #region Enums

        // Module Stages.
        public enum Stages
        {
            // Main Menu.
            MainMenu,
            // Mexican Train Game.
            Game,
        }

        #endregion
    }
}
