using Figgle;

namespace B.Modules.Toys.TextGenerator
{
    public sealed class FontType
    {
        #region Public Properties

        public string Name { get; private set; }
        public FiggleFont Font { get; private set; }

        #endregion



        #region Constructors

        public FontType(string name, FiggleFont font)
        {
            Name = name;
            Font = font;
        }

        #endregion
    }
}
