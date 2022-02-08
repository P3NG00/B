using B.Inputs;
using B.Utils;

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

        public override void Loop()
        {
            switch (this._stage)
            {
                case Stage.MainMenu:
                    {
                        Util.ClearConsole(20, 7);
                        new Input.Option("BrainFuck")
                            .Add(() => this._stage = Stage.Create, "Create", '1')
                            .AddExit(this)
                            .Request();
                    }
                    break;

                case Stage.Create:
                    {
                        Util.ClearConsole(20, 10);
                        new Input.Option("Create")
                            // TODO
                            .Request();
                    }
                    break;
            }
        }

        private enum Stage
        {
            MainMenu,
            Create,
        }
    }
}
