using B.Utils.Themes;

namespace B.Utils
{
    public sealed class Text
    {
        // Private Variables
        private readonly string _string;
        private readonly ColorPair? _colors = null;
        private readonly PrintType? _type = null;

        // Private Constructors
        private Text(object message) => _string = message.ToString() ?? throw new Exception("Message cannot be null!");

        // Public Constructors
        public Text(object message, ColorPair colors) : this(message) => _colors = colors;
        public Text(object message, PrintType type = PrintType.General) : this(message) => _type = type;

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
    }
}
