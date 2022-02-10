using B.Inputs;
using B.Utils;

namespace B.Options.Settings
{
    public sealed class OptionSettings : Option
    {
        private Vector2 _size = new(40, 20);

        private Stage _stage = Stage.MainMenu;

        public override void Loop()
        {
            switch (this._stage)
            {
                case Stage.MainMenu:
                    {
                        Util.ClearConsole(35, 10);
                        Util.PrintLine();
                        Util.PrintLine("  Settings");
                        Util.PrintLine();
                        Util.PrintLine($" Detected Max Size: {Program.WINDOW_SIZE_MAX}");
                        new Input.Option()
                            .Add(() => this._stage = Stage.WindowSize, "Window Size", '1')
                            .Add(() => this._stage = Stage.Color, "Color", '2')
                            .AddExit(this)
                            .Request();
                    }
                    break;

                case Stage.WindowSize:
                    {
                        if (Program.WINDOW_SIZE_MAX is not null)
                            this._size = Vector2.Clamp(this._size, Program.WINDOW_SIZE_MIN, Program.WINDOW_SIZE_MAX);
                        else
                            this._size = Vector2.Max(this._size, Program.WINDOW_SIZE_MIN);

                        Util.ClearConsole(this._size.x, this._size.y);
                        Util.PrintLine($"Width: {this._size.x}");
                        Util.PrintLine($"Height: {this._size.y}");

                        new Input.Option()
                            .Add(() => this._size.x++, keyChar: '8', key: ConsoleKey.RightArrow)
                            .Add(() => this._size.x--, keyChar: '2', key: ConsoleKey.LeftArrow)
                            .Add(() => this._size.y++, keyChar: '6', key: ConsoleKey.DownArrow)
                            .Add(() => this._size.y--, keyChar: '4', key: ConsoleKey.UpArrow)
                            .Add(() => this._stage = Stage.MainMenu, key: ConsoleKey.Escape)
                            .Request();
                    }
                    break;

                case Stage.Color:
                    {
                        Util.ClearConsole(32, 26);
                        Util.PrintLine();
                        Util.PrintLine("  Colors");
                        ConsoleColor[] colors = Enum.GetValues<ConsoleColor>();
                        Input.RequestScroll(colors,
                            color => color.ToString(),
                            colors.Length,
                            new(() => Console.BackgroundColor = colors[Input.ScrollIndex], "Set Background", '1'),
                            new(() => Console.ForegroundColor = colors[Input.ScrollIndex], "Set Foreground", '2'),
                            new(() =>
                            {
                                Input.ScrollIndex = 0;
                                this._stage = Stage.MainMenu;
                            }, "Exit", key: ConsoleKey.Escape));
                    }
                    break;
            }
        }

        private enum Stage
        {
            MainMenu,
            WindowSize,
            Color,
        }
    }
}
