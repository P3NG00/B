using System.Reflection;
using Figgle;

namespace B.Modules.Toys.TextGenerator
{
    // Static class that stores all FontTypes.
    public static class Fonts
    {
        #region Universal Properties

        // FontType array reference.
        public static FontType[] FontArray => _fonts;

        #endregion



        #region Private Variables

        // Stores FontTypes.
        private static readonly FontType[] _fonts;

        #endregion



        #region Constructors

        // Initializes FontTypes from FiggleFonts.
        static Fonts()
        {
            // Get all font properties
            PropertyInfo[] properties = typeof(FiggleFonts).GetProperties();
            // From each property, put font into array
            _fonts = properties.Select(property =>
            {
                string name = property.Name;
                FiggleFont font = (FiggleFont)property.GetValue(null)!;
                FontType fontType = new(name, font);
                return fontType;
            }).ToArray();
            // Sort array by name
            Array.Sort(_fonts, (fontA, fontB) => fontA.Name.CompareTo(fontB.Name));
        }

        #endregion
    }
}
