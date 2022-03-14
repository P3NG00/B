namespace B.Utils
{
    public static class Cursor
    {
        public static Vector2 Position
        {
            get => new(Console.CursorLeft, Console.CursorTop);
            set => Console.SetCursorPosition(value.x, value.y);
        }

        public static int x
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        public static int y
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }

        public static void Reset() => Console.SetCursorPosition(0, 0);
    }
}
