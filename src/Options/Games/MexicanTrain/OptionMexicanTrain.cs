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
        public const int DOMINO_START = 10; // TODO change to take into account the number of players

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
                        Window.Clear();
                        Window.SetSize(34, 7);
                        Cursor.Position = new(0, 1);
                        Input.Choice choice = Input.Choice.Create(OptionMexicanTrain.Title);
                        choice.AddMessage($"Press <{PLAYERS_MIN}-{PLAYERS_MAX}> to select players.");
                        choice.AddSpacer();
                        choice.AddExit(this);
                        choice.AddRoutine(keybinds =>
                        {
                            for (int i = PLAYERS_MIN; i <= PLAYERS_MAX; i++)
                            {
                                // Create new instances because 'i' will change while looping
                                MexicanTrainInfo info = new(i); // TODO change this so it isn't creating multiple instances of MexicanTrainInfo but instead only one once a number is pressed
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
                        choice.Request();
                    }
                    break;

                case Stages.Game:
                    {
                        Window.Clear();
                        Window.SetSize(60, 20);
                        Cursor.Position = new(2, 1);
                        Window.Print($"Players: {_info.Players}");

                        // TODO
                        _info.HandleTurn();

                        // TODO
                    }
                    throw new NotImplementedException();
            }
        }

        public enum Stages
        {
            MainMenu,
            Game,
        }
    }
}
