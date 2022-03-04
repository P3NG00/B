namespace B.Utils
{
    public static class Window
    {
        public static Vector2 SIZE_MIN => new(16, 2);
        public static Vector2 SIZE_MAX => new(Console.LargestWindowWidth, Console.LargestWindowHeight);

        public static void Print(object message, (int, int) position, ConsoleColor? colorText = null, ConsoleColor? colorBG = null)
        {
            Cursor.SetPosition(position);
            Window.Print(message, colorText, colorBG);
        }

        public static void Print(object message, Vector2 position, ConsoleColor? colorText = null, ConsoleColor? colorBG = null)
        {
            Cursor.SetPosition(position);
            Window.Print(message, colorText, colorBG);
        }

        public static void Print(object message, ConsoleColor? colorText = null, ConsoleColor? colorBG = null)
        {
            // Override colors if specified
            if (colorText.HasValue) Console.ForegroundColor = colorText.Value;
            if (colorBG.HasValue) Console.BackgroundColor = colorBG.Value;

            // Print message
            Console.Write(message);

            // Restore old color values if overriden
            Program.Settings.UpdateColors();
        }

        public static void PrintLine(object message = null!, ConsoleColor? colorText = null, ConsoleColor? colorBackground = null)
        {
            if (message != null)
                Window.Print(message, colorText, colorBackground);

            Console.WriteLine();
        }

        public static void PrintLines(int lines)
        {
            for (int i = 0; i < lines; i++)
                Console.WriteLine();
        }

        public static void SetSize(int width, int height)
        {
            // This can only be called on Windows
            if (OperatingSystem.IsWindows())
            {
                Console.SetWindowSize(width, height);
                Console.SetBufferSize(Console.WindowLeft + width, Console.WindowTop + height);
                Console.SetWindowSize(width, height); // This is called twice to fix the scrollbar from showing
            }
        }

        public static void SetSize((int width, int height) size) => Window.SetSize(size.width, size.height);

        public static void SetSize(Vector2 size) => Window.SetSize(size.x, size.y);

        public static void Clear() => Console.Clear();

        public static void ClearAndSetSize(int width, int height)
        {
            Window.Clear();
            Window.SetSize(width, height);
        }

        public static void ClearAndSetSize((int width, int height) size)
        {
            Window.Clear();
            Window.SetSize(size);
        }

        public static void ClearAndSetSize(Vector2 size)
        {
            Window.Clear();
            Window.SetSize(size);
        }
    }
}
