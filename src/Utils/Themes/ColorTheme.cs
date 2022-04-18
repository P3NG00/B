using B.Utils.Extensions;
using Newtonsoft.Json;

namespace B.Utils.Themes
{
    public sealed class ColorTheme
    {
        [JsonIgnore] public readonly string Title;
        public List<PrintPair> PrintPairs { get; private set; } = new();

        public ColorTheme(string title, params PrintPair[] printPairs)
        {
            Title = title;
            printPairs.ForEach(printPair => { PrintPairs.Add(printPair); });
        }

        public ColorPair this[PrintType type]
        {
            get
            {
                ColorPair general = null!;

                foreach (var printPair in PrintPairs)
                {
                    if (printPair.PrintType == type)
                        return printPair.ColorPair;
                    else if (printPair.PrintType == PrintType.General)
                        general = printPair.ColorPair;
                }

                if (general is null)
                    throw new Exception("No general color found.");
                else
                    return general;
            }
        }
    }
}
