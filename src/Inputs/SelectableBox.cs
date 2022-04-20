using B.Utils;

namespace B.Inputs
{
    [Obsolete]
    public sealed class SelectableBox
    {
        public readonly Action Action;

        private readonly Vector2 _position;
        private readonly string _message;

        public bool IsHighlighted
        {
            get
            {
                Vector2 mousePos = Mouse.Position;
                bool inLeftEdge = mousePos.x >= _position.x;
                bool inRightEdge = mousePos.x <= _position.x + _message.Length - 1;
                bool isInX = inLeftEdge && inRightEdge;
                bool isOnY = mousePos.y == _position.y;
                return isInX && isOnY;
            }
        }

        public SelectableBox(Action action, String message, Vector2 position)
        {
            _position = position;
            _message = message;
            Action = action;
        }

        public void Print()
        {
            Cursor.Position = _position;

            if (IsHighlighted)
                Window.Print(_message, PrintType.Highlight);
            else
                Window.Print(_message, PrintType.General);
        }
    }
}
