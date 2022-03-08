using B.Inputs;
using B.Utils;

namespace B.Options.Games.MexicanTrain
{
    public sealed class OptionMexicanTrain : Option<OptionMexicanTrain.Stages>
    {
        public const string Title = "Mexican Train";

        private const byte MAX_DOMINO_VALUES = 13;
        private const byte MAX_DOMINOES = 91;

        private Domino[] _dominoes = null!;

        public OptionMexicanTrain() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            switch (Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.ClearAndSetSize(20, 7);
                        Input.CreateChoice(OptionMexicanTrain.Title)
                            .Add(() =>
                            {
                                InitGame();
                                SetStage(Stages.Game);
                            }, "Play", '1')
                            .AddExit(this)
                            .Request();
                        // TODO
                    }
                    break;

                case Stages.Game:
                    {
                        throw new NotImplementedException();
                        // TODO
                    }
                    break;
            }
        }

        private void InitGame()
        {
            _dominoes = new Domino[0];

            for (byte b1 = 0; b1 < MAX_DOMINO_VALUES; b1++)
                for (byte b2 = b1; b2 < MAX_DOMINO_VALUES; b2++)
                    _dominoes = _dominoes.Add(new Domino(b1, b2));

            _dominoes.Shuffle();
        }

        public enum Stages
        {
            MainMenu,
            Game,
        }
    }
}
