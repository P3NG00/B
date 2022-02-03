namespace B.Options.BrainFuck
{
    public sealed class OptionBrainFuck : Option
    {
        private Stage _stage = Stage.MainMenu;

        /*

        TODO

        1) Create Program
        2) Saved Programs

        */

        public OptionBrainFuck()
        {
        }

        public override void Loop()
        {
            switch (this._stage)
            {
                case Stage.MainMenu:
                    {
                    }
                    break;
            }
        }

        private enum Stage
        {
            MainMenu,
        }
    }
}
