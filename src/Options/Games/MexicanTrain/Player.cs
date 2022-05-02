using B.Utils.Extensions;

namespace B.Options.Games.MexicanTrain
{
    public class Player
    {
        #region Private Variables

        private List<Domino> _hand = new();

        #endregion



        #region Public Methods

        // Returns if the player's turn is over
        public virtual bool HandleTurn()
        {
            throw new NotImplementedException();
            // TODO create input choice for user to choose domino to play
        }

        public bool TakeRandomFrom(List<Domino> list)
        {
            if (!list.IsEmpty())
            {
                _hand.Add(list.RemoveRandom());
                return true;
            }

            return false;
        }

        #endregion
    }
}
