namespace B.Utils
{
    public sealed class Text
    {
        private readonly string _text;
        private readonly ConsoleColor? _colorText;
        private readonly ConsoleColor? _colorBG;

        public Text(string text, ConsoleColor? colorText = null, ConsoleColor? colorBG = null)
        {
            _text = text;
            _colorText = colorText;
            _colorBG = colorBG;
        }

        public void Print() => Window.Print(_text, _colorText, _colorBG);
    }
}
