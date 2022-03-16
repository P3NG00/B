namespace B.Options.Games.MadLibs
{
    public sealed class OptionMadLibs : Option<OptionMadLibs.Stages>
    {
        public static string Title => "Mad-Libs";

        public OptionMadLibs() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            // TODO
            throw new NotImplementedException();
        }

        public enum Stages
        {
            MainMenu,
        }
    }
}
