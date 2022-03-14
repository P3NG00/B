namespace B.Utils
{
    public static class Window
    {
        public static Vector2 SIZE_MIN => new(16, 2);
        public static Vector2 SIZE_MAX => new(Console.LargestWindowWidth, Console.LargestWindowHeight);

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

        // TODO remove this function when all references are gone
        [Obsolete("Use Cursor.SetPosition and Window.Print instead.")]
        public static void PrintLine(object message = null!, ConsoleColor? colorText = null, ConsoleColor? colorBG = null)
        {
            if (message != null)
                Window.Print(message, colorText: colorText, colorBG: colorBG);

            Console.WriteLine();
        }

        public static void SetSize(int width, int height)
        {
            // This can only be called on Windows
            if (OperatingSystem.IsWindows())
            {
                Console.SetWindowSize(width, height);
                Console.SetBufferSize(Console.WindowLeft + width,
                                      Console.WindowTop + height);
                Console.SetWindowSize(width, height); // This is called twice to fix the scrollbar from showing
            }
        }

        public static void SetSize(Vector2 size) => Window.SetSize(size.x, size.y);

        public static void Clear() => Console.Clear();
    }
}
