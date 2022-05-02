namespace B.Options.Tools.Backup
{
    public sealed class OptionBackup : Option<OptionBackup.Stages>
    {
        #region Universal Properties

        public static string Title => "Backup";

        #endregion



        #region Constructors

        public OptionBackup() : base(Stages.MainMenu) { }

        #endregion



        #region Override Methods

        public override void Loop()
        {
            // TODO

            throw new NotImplementedException();

            // switch (Stage)
            // {
            //     case Stages.MainMenu:
            //         {
            //             Window.ClearAndSetSize(20, 10);
            //         }
            //         break;
            // }
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
