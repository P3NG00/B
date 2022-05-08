namespace B.Modules.Games.MadLibs
{
    public sealed class ModuleMadLibs : Module<ModuleMadLibs.Stages>
    {
        #region Universal Properties

        public static string Title => "Mad-Libs";

        #endregion



        #region Constructors

        public ModuleMadLibs() : base(Stages.MainMenu) { }

        #endregion



        #region Override Methods

        public override void Loop()
        {
            // TODO
            throw new NotImplementedException();
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
