using Figgle;

namespace B.Modules.Toys.TextGenerator
{
    public sealed class FontType
    {
        #region Public Properties

        // Name of font.
        public string Name { get; private set; }
        // Font reference.
        public FiggleFont Font { get; private set; }

        #endregion



        #region Constructors

        /// Creates a new FontType.
        public FontType(string name, FiggleFont font)
        {
            Name = name;
            Font = font;
        }

        #endregion
    }
}
