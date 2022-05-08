using System.Reflection;
using B.Utils.Extensions;
using Figgle;

namespace B.Modules.Toys.TextGenerator
{
    public static class Fonts
    {
        #region Universal Properties

        public static FontType[] FontArray => _fonts;

        #endregion



        #region Private Variables

        private static readonly FontType[] _fonts;

        #endregion



        #region Constructors

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
            Array.Sort(_fonts, (fontA, fontB) => fontA.Name.CompareTo(fontB.Name));
        }

        #endregion
    }
}
