namespace B.Utils.Themes
{
    public sealed class PrintPair
    {
        public PrintType PrintType { get; private set; }
        public ColorPair ColorPair { get; private set; }

        public PrintPair(PrintType printType, ColorPair colorPair)
        {
            PrintType = printType;
            ColorPair = colorPair;
        }
    }
}
