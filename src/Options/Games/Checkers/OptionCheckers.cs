namespace B.Options.Games.OptionCheckers
{
    public sealed class OptionCheckers : Option<OptionCheckers.Stages>
    {
        // TODO - intended to be a checkers game. 2 player / 1 vs AI

        public static string Title => "Checkers";

        public OptionCheckers() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            throw new NotImplementedException(); // TODO REMOVE

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

        public enum Stages
        {
            MainMenu,
            Game,
        }
    }
}
