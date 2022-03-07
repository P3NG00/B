using System.Data.SqlTypes;
namespace B.Options.Games.MexicanTrain
{
    public sealed class OptionMexicanTrain : Option<OptionMexicanTrain.Stages>
    {
        public OptionMexicanTrain() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            throw new NotImplementedException();
            // TODO
        }

        public enum Stages
        {
            MainMenu,
        }
    }
}
