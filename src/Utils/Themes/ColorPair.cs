namespace B.Utils.Themes
{
    public sealed class ColorPair
    {
        public ConsoleColor? ColorText { get; private set; }
        public ConsoleColor? ColorBack { get; private set; }

        public ColorPair(ConsoleColor? colorText = null, ConsoleColor? colorBack = null)
        {
            ColorText = colorText;
            ColorBack = colorBack;
        }

        public void SetConsoleColors()
        {
            if (ColorText.HasValue)
                Console.ForegroundColor = ColorText.Value;

            if (ColorBack.HasValue)
                Console.BackgroundColor = ColorBack.Value;
        }
    }
}
