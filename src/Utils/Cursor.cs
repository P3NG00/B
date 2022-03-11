namespace B.Utils
{
    public static class Cursor
    {
        public static Vector2 GetPosition() => new(Console.CursorLeft, Console.CursorTop);

        public static void SetPosition(int x, int y) => Console.SetCursorPosition(x, y);

        public static void SetPosition(Vector2 position) => Console.SetCursorPosition(position.x, position.y);

        public static void Reset() => Console.SetCursorPosition(0, 0);
    }
}
