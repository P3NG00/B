using Figgle;

namespace B.Options.Toys.TextGenerator
{
    public sealed class FontType
    {
        public string Name { get; private set; }
        public FiggleFont Font { get; private set; }

        public FontType(string name, FiggleFont font)
        {
            Name = name;
            Font = font;
        }
    }
}
