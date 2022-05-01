using B.Utils.Enums;
using B.Utils.Themes;

namespace B.Utils
{
    public sealed class Text
    {
        #region Private Variables

        private readonly string _string;
        private readonly ColorPair? _colors = null;
        private readonly PrintType? _type = null;

        #endregion



        #region Constructors

        // Private
        private Text(object message) => _string = message.ToString() ?? throw new Exception("Message cannot be null!");

        // Public
        public Text(object message, ColorPair colors) : this(message) => _colors = colors;
        public Text(object message, PrintType type = PrintType.General) : this(message) => _type = type;

        #endregion



        #region Public Methods

        // Public Methods
        public void Print()
        {
            if (_colors is not null)
                Window.Print(_string, _colors);
            else if (_type is not null)
                Window.Print(_string, _type.Value);
            else
                throw new Exception("Text could not be printed, no colors nor type specified!");
        }

        #endregion
    }
}
