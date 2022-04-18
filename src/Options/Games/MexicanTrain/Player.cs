using B.Inputs;
using B.Utils.Extensions;

namespace B.Options.Games.MexicanTrain
{
    public class Player
    {
        private List<Domino> _hand = new();

        // Returns if the player's turn is over
        public virtual bool HandleTurn()
        {
            Input.Choice choice = Input.Choice.Create();
            // choice.AddKeybind(); // TODO
            choice.Request();

            // TODO create input choice for user to choose domino to play

            // TODO
            return false;
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
    }
}
