namespace B.Utils
{
    public static class Cursor
    {
        public static Vector2 Position
        {
            get => new(Console.CursorLeft, Console.CursorTop);
            set => Console.SetCursorPosition(value.x, value.y);
        }

        public static void Reset() => Cursor.Position = Vector2.Zero;
    }
}
