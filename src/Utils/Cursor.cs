namespace B.Utils
{
    public static class Cursor
    {
        public static void SetPosition(int x, int y) => Console.SetCursorPosition(x, y);

        public static void SetPosition(Vector2 position) => Cursor.SetPosition(position.x, position.y);

        public static void Reset() => Cursor.SetPosition(Vector2.Zero);
    }
}
