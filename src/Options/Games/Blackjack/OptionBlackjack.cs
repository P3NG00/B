namespace B.Options.Games.Blackjack
{
    public sealed class OptionBlackjack : Option<OptionBlackjack.Stages>
    {
        public const string Title = "Blackjack";

        public OptionBlackjack() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            throw new NotImplementedException();
            // TODO
        }

        public enum Stages
        {
            MainMenu,
            // TODO
        }
    }
}
