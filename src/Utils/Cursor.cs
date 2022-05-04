using B.Utils.Extensions;

namespace B.Utils
{
    public static class Cursor
    {
        #region Universal Properties

        public static Vector2 Position
        {
            get => new(x, y);
            set => Set(value.x, value.y);
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

        public static int Size
        {
            get => Console.CursorSize;
            set
            {
                if (OperatingSystem.IsWindows())
                {
                    int v = value.Clamp(1, 100);
                    Console.CursorSize = v;
                    Program.Settings.CursorSize = v;
                }
            }
        }

        public static bool Visible
        {
            get => OperatingSystem.IsWindows() ? Console.CursorVisible : false;
            set => Console.CursorVisible = value;
        }

        #endregion


        #region Universal Methods

        public static void Reset() => Console.SetCursorPosition(0, 0);

        public static void Set(int x, int y) => Console.SetCursorPosition(x, y);

        public static void NextLine(int x = 0, int lines = 1)
        {
            Cursor.y += lines;
            Cursor.x = x;
        }

        #endregion
    }
}
