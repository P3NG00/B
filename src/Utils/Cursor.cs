namespace B.Utils
{
    public static class Cursor
    {
        // TODO remove
        [Obsolete("Use Cursor.Position instead.")]
        public static Vector2 GetPosition() => new(Console.CursorLeft, Console.CursorTop);

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

        public static Vector2 Position
        {
            get => new(x, y);
            set => Console.SetCursorPosition(value.x, value.y);
        }

        // TODO remove
        [Obsolete("Use Cursor.x & Cursor.y or Cursor.Position instead.")]
        public static void SetPosition(int x, int y) => Console.SetCursorPosition(x, y);

        // TODO remove
        [Obsolete("Use Cursor.Position instead.")]
        public static void SetPosition(Vector2 position) => Console.SetCursorPosition(position.x, position.y);

        // TODO may just be able to replace with Cursor.SetPosition(0, 0)
        public static void Reset() => Console.SetCursorPosition(0, 0);
    }
}
