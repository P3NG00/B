using B.Inputs;
using B.Utils;

namespace B.Options.Games.MexicanTrain
{
    public sealed class OptionMexicanTrain : Option<OptionMexicanTrain.Stages>
    {
        public const string Title = "Mexican Train";

        private const int PLAYERS_MIN = 2;
        private const int PLAYERS_MAX = 8;

        private MexicanTrainInfo _info = null!;

        public OptionMexicanTrain() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.ClearAndSetSize(32, 7);
                        Input.CreateChoice(OptionMexicanTrain.Title, $"Press <{OptionMexicanTrain.PLAYERS_MIN}-{OptionMexicanTrain.PLAYERS_MAX}> to select players.")
                            .AddRoutine(keybinds =>
                            {
                                for (int i = PLAYERS_MIN; i <= PLAYERS_MAX; i++)
                                {
                                    MexicanTrainInfo info = new(i);
                                    char c = (char)('0' + i);

                                    keybinds.Add(new(() =>
                                    {
                                        _info = info;
                                        SetStage(Stages.Game);
                                    }, keyChar: c));
                                }
                            })
                            .AddExit(this)
                            .Request();
                        // TODO
                    }
                    break;

                case Stages.Game:
                    {
                        Window.ClearAndSetSize(60, 20);
                        Window.Print($"Players: {_info.Players}", (2, 1));
                        Input.WaitFor(ConsoleKey.Escape);
                        // TODO

                        // TODO
                        throw new NotImplementedException();
                    }
                    break;
            }
        }

        [Obsolete("Not yet implemented.", true)]
        public override void Save()
        {
            throw new NotImplementedException();
        }

        public enum Stages
        {
            MainMenu,
            Game,
        }
    }
}
