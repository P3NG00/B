using B.Inputs;
using B.Utils;

namespace B.Options.Debug
{
    public sealed class OptionDebug : Option
    {
        private readonly Vector2 _size = new Vector2(40, 20);

        public override void Loop()
        {
            Console.Clear();
            Util.SetConsoleSize(this._size.x, this._size.y);
            Util.Print($"Width: {this._size.x}");
            Util.Print($"Height: {this._size.y}");

            new Input.Option()
                .AddKeybind(new Keybind(() => this._size.x++, keyChar: '8', key: ConsoleKey.RightArrow))
                .AddKeybind(new Keybind(() => this._size.x--, keyChar: '2', key: ConsoleKey.LeftArrow))
                .AddKeybind(new Keybind(() => this._size.y++, keyChar: '6', key: ConsoleKey.DownArrow))
                .AddKeybind(new Keybind(() => this._size.y--, keyChar: '4', key: ConsoleKey.UpArrow))
                .Request();
        }
    }
}
