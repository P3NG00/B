namespace B.Modules.Games.MadLibs
{
    public sealed class ModuleMadLibs : Module<ModuleMadLibs.Stages>
    {
        #region Universal Properties

        // Module Title.
        public static string Title => "Mad-Libs";

        #endregion



        #region Constructors

        // Creates a new instance of ModuleMadLibs.
        public ModuleMadLibs() : base(Stages.MainMenu) { }

        #endregion



        #region Override Methods

        // Module Loop.
        public override void Loop()
        {
            // TODO
            throw new NotImplementedException();
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
