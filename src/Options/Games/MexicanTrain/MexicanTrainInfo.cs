using B.Utils;

namespace B.Options.Games.MexicanTrain
{
    [Serializable]
    public sealed class MexicanTrainInfo
    {
        // Main domino pool
        private List<Domino> _dominoes = new();
        // Mexican train
        private List<Domino> _mexicanTrain = new();
        // Hands for players
        private List<Domino>[] _hands;
        // Current player index
        private int _player = 0;
        // Round counter
        private int _round = 0;

        public int Players => _hands.Length;

        public MexicanTrainInfo(int amountOfPlayers)
        {
            _hands = new List<Domino>[amountOfPlayers];

            for (int i = 0; i < amountOfPlayers; i++)
                _hands[i] = new();
        }

        public void ResetHands()
        {
            // Clear hands
            _dominoes.Clear();
            _mexicanTrain.Clear();
            _hands.ForEach(hand => hand.Clear());

            // Create dominoes
            for (int i1 = 0; i1 <= Constants.MAX_DOMINO_VALUES; i1++)
                for (int i2 = i1; i2 <= Constants.MAX_DOMINO_VALUES; i2++)
                    if (i1 != _round && i2 != _round)
                        _dominoes.Add(new Domino(i1, i2));

            // Shuffle dominoes
            _dominoes.Shuffle();

            // Deal dominoes
            for (int i = 0; i < 10; i++) // TODO change amount of dominoes taken based on amount of players
                foreach (List<Domino> hand in _hands)
                    _dominoes.MoveFirstTo(hand);
        }
    }
}
