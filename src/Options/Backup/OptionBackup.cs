using System.Security.Principal;
using B.Utils;

namespace B.Options.Backup
{
    public sealed class OptionBackup : Option<OptionBackup.Stages>
    {
        public const string Title = "Backup";

        public OptionBackup() : base(Stages.MainMenu) { }

        public override void Loop()
        {
            switch (this.Stage)
            {
                case Stages.MainMenu:
                    {
                        Window.ClearAndSetSize(20, 10);
                        // TODO
                    }
                    break;

                case Stages.MC:
                    {
                        // TODO
                    }
                    break;
            }

            // TODO remove this, this is only to exit from this option
            this.Quit();
        }

        public enum Stages
        {
            MainMenu,
            MC,
        }
    }
}
