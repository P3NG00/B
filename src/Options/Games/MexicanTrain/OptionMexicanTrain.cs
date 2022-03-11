using B.Inputs;
using B.Utils;

namespace B.Options.Games.MexicanTrain
{
    public sealed class OptionMexicanTrain : Option<OptionMexicanTrain.Stages>
    {
        public const string Title = "Mexican Train";
        public const int DOMINO_MAX = 12;
        public const int PLAYERS_MIN = 2;
        public const int PLAYERS_MAX = 8;
        public const int DOMINO_START = 10; // TODO change to account for number of players

        // TODO implement saving/loading
        public static string FilePath => Program.DataPath + "mexicanTrain";

        private MexicanTrainInfo _info = null!;

        public OptionMexicanTrain() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.ClearAndSetSize(34, 7);
                        Input.Choice choice = Input.Choice.Create(OptionMexicanTrain.Title);
                        choice.AddMessageSpacer();
                        choice.AddMessage($"Press <{PLAYERS_MIN}-{PLAYERS_MAX}> to select players.");
                        choice.AddRoutine(keybinds =>
                        {
                            for (int i = PLAYERS_MIN; i <= PLAYERS_MAX; i++)
                            {
                                // Create new instances because 'i' will change while looping
                                MexicanTrainInfo info = new(i);
                                char c = (char)('0' + i);

                                // Add keybinds
                                keybinds.Add(new(() =>
                                {
                                    _info = info;
                                    _info.Initialize();
                                    SetStage(Stages.Game);
                                }, keyChar: c));
                            }
                        });
                        choice.AddExit(this);
                        choice.Request();
                    }
                    break;

                case Stages.Game:
                    {
                        Window.ClearAndSetSize(60, 20);
                        Window.Print($"Players: {_info.Players}", (2, 1));

                        // TODO
                        _info.HandleTurn();
                    }
                    break;
            }
        }

        public enum Stages
        {
            MainMenu,
            Game,
        }
    }
}
