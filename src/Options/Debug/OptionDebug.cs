using B.Inputs;
using B.Utils;

namespace B.Options.Debug
{
    public sealed class OptionDebug : Option
    {
        private Vector2 _size = new(40, 20);

        private Stage _stage = Stage.MainMenu;

        public override void Loop()
        {
            switch (this._stage)
            {
                case Stage.MainMenu:
                    {
                        Util.ClearConsole(20, 8);
                        new Input.Option("DEBUG")
                            .AddKeybind(new(() => this._stage = Stage.WindowSize, "Window Size", '1'))
                            .AddKeybind(new(() => this._stage = Stage.Color, "Color", '2'))
                            .AddSpacer()
                            .AddKeybind(new(() => this.Quit(), "Back", key: ConsoleKey.Escape))
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
                            .AddKeybind(new(() => this._size.x++, keyChar: '8', key: ConsoleKey.RightArrow))
                            .AddKeybind(new(() => this._size.x--, keyChar: '2', key: ConsoleKey.LeftArrow))
                            .AddKeybind(new(() => this._size.y++, keyChar: '6', key: ConsoleKey.DownArrow))
                            .AddKeybind(new(() => this._size.y--, keyChar: '4', key: ConsoleKey.UpArrow))
                            .AddKeybind(new(() => this._stage = Stage.MainMenu, key: ConsoleKey.Escape))
                            .Request();
                    }
                    break;

                case Stage.Color:
                    {
                        // TODO implement color options
                        // Input Scroll selection using Enum.GetValues<ConsoleColor>()
                        // use two keybinds, one for each: foreground, background
                        // when keybind is pressed, appropriate color from index is set to fore/back ground
                        this._stage = Stage.MainMenu;
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
