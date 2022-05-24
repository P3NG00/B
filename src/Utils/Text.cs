using B.Utils.Enums;
using B.Utils.Themes;

namespace B.Utils
{
    public sealed class Text
    {
        #region Private Variables

        // String of text for printing.
        private readonly string _string;
        // ColorPair to use for printing.
        private readonly ColorPair? _colors = null;
        // PrintType to use for printing.
        private readonly PrintType? _printType = null;

        #endregion



        #region Constructors

        // Constructor that sets the text to print.
        private Text(object message) => _string = message.ToString() ?? throw new Exception("Message cannot be null!");

        // Creates Text to print with specified ColorPair.
        public Text(object message, ColorPair colors) : this(message) => _colors = colors;
        // Creates Text to print with specified PrintType.
        public Text(object message, PrintType printType = PrintType.General) : this(message) => _printType = printType;

        #endregion



        #region Public Methods

        // Prints stored text with either specified ColorPair or PrintType.
        public void Print()
        {
            if (_colors is not null)
                Window.Print(_string, _colors);
            else if (_printType is not null)
                Window.Print(_string, _printType.Value);
            else
                throw new Exception("Text could not be printed, no colors nor type specified!");
        }

        #endregion
    }
}
