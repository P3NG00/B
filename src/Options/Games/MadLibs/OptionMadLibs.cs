namespace B.Options.Games.MadLibs
{
    public sealed class OptionMadLibs : Option<OptionMadLibs.Stages>
    {
        #region Universal Properties

        public static string Title => "Mad-Libs";

        #endregion



        #region Constructors

        public OptionMadLibs() : base(Stages.MainMenu) { }

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
