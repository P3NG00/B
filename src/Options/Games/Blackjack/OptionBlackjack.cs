namespace B.Options.Games.Blackjack
{
    public sealed class OptionBlackjack : Option<OptionBlackjack.Stages>
    {
        #region Universal Properties

        public static string Title => "Blackjack";

        #endregion



        #region Constructors

        public OptionBlackjack() : base(Stages.MainMenu) { }

        #endregion



        #region Override Methods

        public override void Loop()
        {
            throw new NotImplementedException();
            // TODO
        }

        #endregion



        #region Enums

        public enum Stages
        {
            MainMenu,
        }

        #endregion
    }
}
