using B.Utils.Enums;

namespace B.Utils.Themes
{
    public sealed class PrintPair
    {
        #region Public Properties

        // PrintType key.
        public PrintType PrintType { get; private set; }

        #endregion



        #region Public Variables

        // ColorPair value.
        public ColorPair ColorPair;

        #endregion



        #region Constructor

        // Creates a new PrintPair with specified PrintType and ColorPair.
        public PrintPair(PrintType printType, ColorPair colorPair)
        {
            PrintType = printType;
            ColorPair = colorPair;
        }

        #endregion
    }
}
