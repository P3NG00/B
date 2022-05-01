using B.Utils.Extensions;
using Newtonsoft.Json;

namespace B.Utils.Themes
{
    public sealed class ColorTheme
    {
        [JsonIgnore] public readonly string Title;

        // This variables is kept public for serialization purposes
        public List<PrintPair> PrintPairs { get; private set; } = new();
        private PrintPair _printTypeGeneralPairReference;

        public ColorTheme(string title, params PrintPair[] printPairs)
        {
            Title = title;

            // Setup check
            PrintPair generalPair = null!;
            bool hasHighlightPair = false;

            // Add each PrintPair to the list.
            printPairs.ForEach(printPair =>
            {
                // Check existing entries to ensure this PrintPair contains a unique PrintType.
                PrintPair foundPrintPair = GetPrintPair(printPair.PrintType);

                // If PrintTypes already exists, throw exception.
                if (foundPrintPair is not null)
                    throw new Exception($"PrintPair with PrintType '{printPair.PrintType}' already exists!");

                // PrintPair does not contain a duplicate PrintType, add to the list
                PrintPairs.Add(printPair);

                // Check if this PrintPair satisfies conditions
                if (generalPair is null && printPair.PrintType == PrintType.General)
                    generalPair = printPair;
                if (!hasHighlightPair && printPair.PrintType == PrintType.Highlight)
                    hasHighlightPair = true;
            });

            // Ensure a general pair was added.
            if (generalPair is null)
                throw new Exception("ColorTheme must contain a PrintPair with PrintType 'General'!");

            _printTypeGeneralPairReference = generalPair;

            // Check if highlight was specified.
            if (!hasHighlightPair)
            {
                // Create inverted version of general pair.
                ColorPair generalColorPair = generalPair.ColorPair;
                ColorPair invertedGeneralColorPair = new(generalColorPair.ColorBack, generalColorPair.ColorText);
                PrintPair highlightPair = new PrintPair(PrintType.Highlight, invertedGeneralColorPair);

                // Add highlight pair to list.
                PrintPairs.Add(highlightPair);
            }
        }

        private PrintPair GetPrintPair(PrintType printType)
        {
            try { return PrintPairs.First(printPair => printPair.PrintType == printType); }
            catch (Exception) { return null!; }
        }

        public ColorPair this[PrintType type] => PrintPairs.FirstOrDefault(printPair => printPair.PrintType == type, _printTypeGeneralPairReference).ColorPair;
    }
}
