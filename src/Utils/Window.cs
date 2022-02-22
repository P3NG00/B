namespace B.Utils
{
    public static class Window
    {
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

        public static void Print(object message, ConsoleColor? colorText = null, ConsoleColor? colorBackground = null)
        {
            // Cache old color values
            ConsoleColor? oldColorText = null;
            ConsoleColor? oldColorBackground = null;

            // Override colors if specified
            if (colorText.HasValue)
            {
                oldColorText = Console.ForegroundColor;
                Console.ForegroundColor = colorText.Value;
            }

            if (colorBackground.HasValue)
            {
                oldColorBackground = Console.BackgroundColor;
                Console.BackgroundColor = colorBackground.Value;
            }

            // Print message
            Console.Write(message);

            // Restore old color values if overriden
            if (oldColorText.HasValue)
                Console.ForegroundColor = oldColorText.Value;

            if (oldColorBackground.HasValue)
                Console.BackgroundColor = oldColorBackground.Value;
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

        public static void PrintSpaces(int spaces) => Window.Print(Util.Spaces(spaces));

        public static void Clear() => Console.Clear();

        public static void ClearAndSetSize(int width, int height)
        {
            Console.Clear();
            Window.Size = new(Math.Clamp(width, Program.WINDOW_MIN.x, Program.WINDOW_MAX.x), Math.Clamp(height, Program.WINDOW_MIN.y, Program.WINDOW_MAX.y));
        }

        public static void ClearAndSetSize(Vector2 size) => Window.ClearAndSetSize(size.x, size.y);
    }
}
