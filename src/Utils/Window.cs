namespace B.Utils
{
    public static class Window
    {
        public static Vector2 SIZE_MIN => new(16, 2);
        public static Vector2 SIZE_MAX => new(Console.LargestWindowWidth, Console.LargestWindowHeight);

        public static (int, int) Size
        {
            get => new(Console.WindowWidth, Console.WindowHeight);
            set
            {
                // This can only be called on Windows
                if (OperatingSystem.IsWindows())
                {
                    int width = value.Item1;
                    int height = value.Item2;
                    Console.SetWindowSize(width, height);
                    Console.SetBufferSize(Console.WindowLeft + width, Console.WindowTop + height);
                    // This is called twice to fix the scrollbar from showing
                    Console.SetWindowSize(width, height);
                }
            }
        }

        public static void Print(object message, (int, int) position, ConsoleColor? colorText = null, ConsoleColor? colorBG = null) => Window.Print(message, new Vector2(position), colorText, colorBG);

        public static void Print(object message, Vector2 position, ConsoleColor? colorText = null, ConsoleColor? colorBG = null)
        {
            Cursor.SetPosition(position);
            Window.Print(message, colorText, colorBG);
        }

        public static void Print(object message, ConsoleColor? colorText = null, ConsoleColor? colorBG = null)
        {
            // Override colors if specified
            if (colorText.HasValue)
                Console.ForegroundColor = colorText.Value;

            if (colorBG.HasValue)
                Console.BackgroundColor = colorBG.Value;

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

        public static void Clear() => Console.Clear();

        public static void ClearAndSetSize(int width, int height)
        {
            Console.Clear();
            Window.Size = new(Math.Clamp(width, Window.SIZE_MIN.x, Window.SIZE_MAX.x), Math.Clamp(height, Window.SIZE_MIN.y, Window.SIZE_MAX.y));
        }

        public static void ClearAndSetSize(Vector2 size) => Window.ClearAndSetSize(size.x, size.y);
    }
}
