namespace B.Utils.Themes
{
    public sealed class ColorPair
    {
        #region Public Properties

        public ConsoleColor? ColorText { get; private set; }
        public ConsoleColor? ColorBack { get; private set; }

        #endregion



        #region Constructors

        public ColorPair(ConsoleColor? colorText = null, ConsoleColor? colorBack = null)
        {
            ColorText = colorText;
            ColorBack = colorBack;
        }

        #endregion



        #region Public Methods

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
