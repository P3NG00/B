using B.Inputs;
using B.Utils;

namespace B.Options.Debug
{
    public sealed class OptionDebug : Option
    {
        private static readonly Vector2 _size = new Vector2(40, 20);

        public override void Loop()
        {
            Util.ClearConsole(OptionDebug._size.x, OptionDebug._size.y);
            Util.Print($"Width: {OptionDebug._size.x}");
            Util.Print($"Height: {OptionDebug._size.y}");

            new Input.Option()
                .AddKeybind(new Keybind(() => OptionDebug._size.x++, keyChar: '8', key: ConsoleKey.RightArrow))
                .AddKeybind(new Keybind(() => OptionDebug._size.x--, keyChar: '2', key: ConsoleKey.LeftArrow))
                .AddKeybind(new Keybind(() => OptionDebug._size.y++, keyChar: '6', key: ConsoleKey.DownArrow))
                .AddKeybind(new Keybind(() => OptionDebug._size.y--, keyChar: '4', key: ConsoleKey.UpArrow))
                .Request();
        }
    }
}
