namespace B.Modules.Games.Blackjack
{
    public sealed class ModuleBlackjack : Module<ModuleBlackjack.Stages>
    {
        #region Universal Properties

        public static string Title => "Blackjack";

        #endregion



        #region Constructors

        public ModuleBlackjack() : base(Stages.MainMenu) { }

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
