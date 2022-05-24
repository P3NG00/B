namespace B.Modules.Games.MexicanTrain
{
    public sealed class PlayerAI : Player
    {
        #region Override Methods

        // Returns true if the AI's turn is over.
        public override bool HandleTurn()
        {
            throw new NotImplementedException();

            /*
            // TODO
            * Find all playable dominos in hand.
            * For each playable domino, try creating paths with other dominos in hand.
            * Rank paths by score. paths with the highest score should be played first.
            * Play the highest ranked path.
            * If cannot play, take a domino from the pool.
            */
        }

        #endregion
    }
}
