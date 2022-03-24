using B.Utils;
using B.Utils.Extensions;

namespace B.Options.Games.MexicanTrain
{
    public sealed class MexicanTrainInfo
    {
        // Main domino pool
        private List<Domino> _dominoes = new();
        // Mexican train
        private List<Domino> _mexicanTrain = new();
        // Hands for players
        private Player[] _players;
        // Current player index
        private int _player = 0;
        // Round counter
        private int _round = OptionMexicanTrain.DOMINO_MAX; // TODO count down the rounds

        // Amount of players in game
        public int Players => _players.Length;

        public MexicanTrainInfo(int amountOfPlayers)
        {
            // Create player array
            _players = new Player[amountOfPlayers];

            // TODO account for multiple Human players

            // Make player 1 the Human
            _players[0] = new Player();

            // Make the rest of the players AI
            for (int i = 1; i < amountOfPlayers; i++)
                _players[i] = new PlayerAI();
        }

        public void SetupGame()
        {
            _dominoes.Clear();

            // Create dominoes
            for (int i1 = 0; i1 <= OptionMexicanTrain.DOMINO_MAX; i1++)
            {
                // Iterate through one less to avoid duplicates
                for (int i2 = i1; i2 <= OptionMexicanTrain.DOMINO_MAX; i2++)
                {
                    // Remove the domino with both values equal to the round because
                    // that domino is used in the center as the beginning domino
                    if (i1 != _round && i2 != _round)
                    {
                        // Create and add new domino
                        _dominoes.Add(new Domino(i1, i2));
                    }
                }
            }

            // Shuffle dominoes
            _dominoes.Shuffle();

            // Deal dominoes
            Util.Loop(OptionMexicanTrain.DOMINO_START, () => _players.ForEach(player => player.TakeRandomFrom(_dominoes)));
        }

        public void HandleTurn()
        {
            // TODO

            // Handle current players turn
            bool finishedTurn = _players[_player].HandleTurn();

            // Move to next player
            if (finishedTurn)
                _player = (_player + 1) % Players;
        }
    }
}
