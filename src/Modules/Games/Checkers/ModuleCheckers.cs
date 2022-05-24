namespace B.Modules.Games.Checkers
{
    public sealed class ModuleCheckers : Module<ModuleCheckers.Stages>
    {
        #region Universal Properties

        // Module Title.
        public static string Title => "Checkers";

        #endregion



        #region Constructors

        // Creates a new instance of ModuleCheckers.
        public ModuleCheckers() : base(Stages.MainMenu) { }

        #endregion



        #region Override Methods

        // Module Loop.
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

        // Module Stages.
        public enum Stages
        {
            // Main Menu.
            MainMenu,
            // Game.
            Game,
        }

        #endregion
    }
}
