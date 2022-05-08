namespace B.Modules.Games.MexicanTrain
{
    public sealed class PlayerAI : Player
    {
        #region Override Methods

        public override bool HandleTurn()
        {
            throw new NotImplementedException();

            // TODO

            // TODO find all playable dominos in hand

            // TODO for each playable domino, try creating paths with other dominos in hand

            // TODO rank paths by score. paths with the highest score should be played first

            // TODO play the highest ranked path

            // TODO if cannot play, take a domino from the pool
        }

        #endregion
    }
}
