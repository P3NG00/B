using B.Utils.Extensions;

namespace B.Utils
{
    public static class Cursor
    {
        #region Universal Properties

        // Retrieve and set the position of the text cursor.
        public static Vector2 Position
        {
            get => new(x, y);
            set => Set(value.x, value.y);
        }

        // Retrieve and set the X position of the text cursor.
        public static int x
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        // Retrieve and set the Y position of the text cursor.
        public static int y
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }

        // Retrieve and set the size of the text cursor when it's visible.
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

        // Retrieve and set the visibility of the text cursor.
        public static bool Visible
        {
            get => OperatingSystem.IsWindows() ? Console.CursorVisible : false;
            set => Console.CursorVisible = value;
        }

        #endregion



        #region Universal Methods

        // Resets the position of the text cursor to the top-left corner of the console window. (0, 0)
        public static void Reset() => Console.SetCursorPosition(0, 0);

        // Sets the position of the text cursor.
        public static void Set(int x, int y) => Console.SetCursorPosition(x, y);

        // Adjusts the position of the text cursor.
        public static void NextLine(int x = 0, int lines = 1)
        {
            Cursor.y += lines;
            Cursor.x = x;
        }

        #endregion
    }
}
