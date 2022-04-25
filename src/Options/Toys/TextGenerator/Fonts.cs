using System.Reflection;
using B.Utils.Extensions;
using Figgle;

namespace B.Options.Toys.TextGenerator
{
    public static class Fonts
    {
        private static readonly FontType[] _fonts;

        public static FontType[] FontArray => _fonts;

        static Fonts()
        {
            // Get all font properties
            PropertyInfo[] properties = typeof(FiggleFonts).GetProperties();
            // From each property, put font into array
            _fonts = properties.FromEach(property =>
            {
                string name = property.Name;
                FiggleFont font = (FiggleFont)property.GetValue(null)!;
                FontType fontType = new(name, font);
                return fontType;
            }).ToArray();
            // Sort array by name
            System.Array.Sort(_fonts, (fontA, fontB) => fontA.Name.CompareTo(fontB.Name));
        }
    }
}
