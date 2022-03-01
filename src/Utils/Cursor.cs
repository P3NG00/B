using System.Numerics;
namespace B.Utils
{
    public static class Cursor
    {
        public static (int x, int y) GetPositionTuple() => (Console.CursorLeft, Console.CursorTop);

        public static Vector2 GetPositionVector() => new(Console.CursorLeft, Console.CursorTop);

        public static void SetPosition(int x, int y) => Console.SetCursorPosition(x, y);

        public static void SetPosition(Vector2 position) => Console.SetCursorPosition(position.x, position.y);

        public static void SetPosition((int x, int y) position) => Console.SetCursorPosition(position.x, position.y);

        public static void Reset() => Console.SetCursorPosition(0, 0);
    }
}
