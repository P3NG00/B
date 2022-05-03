using B.Utils.Enums;

namespace B.Utils.Themes
{
    public sealed class PrintPair
    {
        #region Public Properties

        public PrintType PrintType { get; private set; }

        #endregion



        #region Public Variables

        public ColorPair ColorPair;

        #endregion



        #region Constructor

        public PrintPair(PrintType printType, ColorPair colorPair)
        {
            PrintType = printType;
            ColorPair = colorPair;
        }

        #endregion
    }
}
