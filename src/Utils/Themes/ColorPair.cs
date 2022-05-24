namespace B.Utils.Themes
{
    public sealed class ColorPair
    {
        #region Public Variables

        // ConsoleColor used for Text.
        public ConsoleColor? ColorText;
        // ConsoleColor used for Background.
        public ConsoleColor? ColorBack;

        #endregion



        #region Constructors

        // Creates a new ColorPair.
        public ColorPair(ConsoleColor? colorText = null, ConsoleColor? colorBack = null)
        {
            ColorText = colorText;
            ColorBack = colorBack;
        }

        #endregion



        #region Public Methods

        // Sets the current console colors to this ColorPair.
        public void SetConsoleColors()
        {
            if (ColorText.HasValue)
                Console.ForegroundColor = ColorText.Value;

            if (ColorBack.HasValue)
                Console.BackgroundColor = ColorBack.Value;
        }

        #endregion
    }
}
