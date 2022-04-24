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
            // TODO make array only of fonts that display properly
            // Get all font properties
            PropertyInfo[] properties = typeof(FiggleFonts).GetProperties();
            // From each property, put font into array
            _fonts = properties.FromEach(property => new FontType(property.Name, (FiggleFont)property.GetValue(null)!)).ToArray();
            // Sort array by name
            System.Array.Sort(_fonts, (fontA, fontB) => fontA.Name.CompareTo(fontB.Name));
        }
    }
}
