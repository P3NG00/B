using B.Utils.Enums;
using Newtonsoft.Json;

namespace B.Utils.Themes
{
    public sealed class ColorTheme
    {
        #region Public Variables

        // The title of this theme.
        [JsonIgnore] public readonly string Title;

        #endregion



        #region Public Properties

        // List of PrintPair used to find what ColorPair to use for a given PrintType.
        public List<PrintPair> PrintPairs { get; private set; } = new();

        #endregion



        #region Private Variables

        // Reference of General PrintPair for easier referencing.
        private readonly PrintPair _generalPrintPair;

        #endregion



        #region Constructor

        // Creates a new ColorTheme.
        // MUST define a PrintPair with PrintType.General or an exception will be thrown.
        // If PrintType.Highlight is not defined, it will be created automatically by swapping the PrintType.General PrintPair colors.
        public ColorTheme(string title, params PrintPair[] printPairs)
        {
            Title = title;
            PrintPair generalPair = null!;
            bool hasHighlightPair = false;

            // Add each PrintPair to the list.
            foreach (var printPair in printPairs)
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
            }

            // Ensure a general pair was added.
            if (generalPair is null)
                throw new Exception("ColorTheme must contain a PrintPair with PrintType 'General'!");

            // Set reference of general pair.
            _generalPrintPair = generalPair;

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

        // Retrieve and set ColorPair for the given PrintType.
        public ColorPair this[PrintType printType]
        {
            get => GetPrintPair(printType, _generalPrintPair).ColorPair;
            set => GetPrintPair(printType, _generalPrintPair).ColorPair = value;
        }

        #endregion



        #region Private Methods

        // Retrieves the appropriate PrintPair.
        private PrintPair GetPrintPair(PrintType printType, PrintPair defaultPair = null!)
        {
            try { return PrintPairs.First(printPair => printPair.PrintType == printType); }
            catch (Exception) { return defaultPair; }
        }

        #endregion
    }
}
