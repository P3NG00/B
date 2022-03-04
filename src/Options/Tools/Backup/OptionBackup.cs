using B.Utils;

namespace B.Options.Tools.Backup
{
    public sealed class OptionBackup : Option<OptionBackup.Stages>
    {
        public const string Title = "Backup";

        public OptionBackup() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            throw new NotImplementedException();

            // switch (this.Stage)
            // {
            //     case Stages.MainMenu:
            //         {
            //             Window.ClearAndSetSize(20, 10);
            //             // TODO
            //         }
            //         break;

            //     case Stages.MC:
            //         {
            //             // TODO
            //         }
            //         break;
            // }
        }

        public enum Stages
        {
            MainMenu,
            MC,
        }
    }
}
