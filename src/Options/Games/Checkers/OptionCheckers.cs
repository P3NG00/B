namespace B.Options.Games.OptionCheckers
{
    public sealed class OptionCheckers : Option<OptionCheckers.Stages>
    {
        #region Universal Properties

        public static string Title => "Checkers";

        #endregion



        #region Constructors

        public OptionCheckers() : base(Stages.MainMenu) { }

        #endregion



        #region Override Methods

        public override void Loop()
        {
            // TODO - intended to be a checkers game. 2 player / 1 vs AI

            throw new NotImplementedException();

            // switch (Stage)
            // {
            //     case Stages.MainMenu:
            //         {
            //             Window.ClearAndSetSize(80, 25);
            //         }
            //         break;

            //     case Stages.Game:
            //         {
            //         }
            //         break;
        }

        #endregion



        #region Enums

        public enum Stages
        {
            MainMenu,
            Game,
        }

        #endregion
    }
}
