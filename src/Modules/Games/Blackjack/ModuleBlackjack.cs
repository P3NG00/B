namespace B.Modules.Games.Blackjack
{
    public sealed class ModuleBlackjack : Module<ModuleBlackjack.Stages>
    {
        #region Universal Properties

        // Module Title.
        public static string Title => "Blackjack";

        #endregion



        #region Constructors

        // Creates a new instance of ModuleBlackjack.
        public ModuleBlackjack() : base(Stages.MainMenu) { }

        #endregion



        #region Override Methods

        // Module Loop.
        public override void Loop()
        {
            throw new NotImplementedException();
            // TODO
        }

        #endregion



        #region Enums

        // Module Stages.
        public enum Stages
        {
            // Main Menu.
            MainMenu,
        }

        #endregion
    }
}
