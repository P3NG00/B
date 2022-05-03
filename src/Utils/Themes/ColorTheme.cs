using B.Utils.Enums;
using B.Utils.Extensions;
using Newtonsoft.Json;

namespace B.Utils.Themes
{
    public sealed class ColorTheme
    {
        #region Public Variables

        // This does not need to be serialized.
        [JsonIgnore] public readonly string Title;

        #endregion



        #region Public Properties

        // This variable is kept public for serialization purposes.
        public List<PrintPair> PrintPairs { get; private set; } = new();

        #endregion



        #region Private Variables

        // This variable is kept as an easy reference to the PrintPair for PrintType.General.
        private PrintPair _PrintType_GeneralPair;

        #endregion



        #region Constructor

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

            // Set reference of general pair.
            _PrintType_GeneralPair = generalPair;

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

        #endregion



        #region Public Methods

        public ColorPair this[PrintType type]
        {
            get => GetPrintPair(type, _PrintType_GeneralPair).ColorPair;
            set => GetPrintPair(type, _PrintType_GeneralPair).ColorPair = value;
        }

        #endregion



        #region Private Methods

        private PrintPair GetPrintPair(PrintType printType, PrintPair defaultPair = null!)
        {
            try { return PrintPairs.First(printPair => printPair.PrintType == printType); }
            catch (Exception) { return defaultPair; }
        }

        #endregion
    }
}
