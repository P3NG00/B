using B.Utils.Themes;

namespace B.Utils
{
    public static class Window
    {
        #region Universal Properties

        // This represents the size of one char in the console
        public static Vector2 CharSize => new(8, 16);
        // The minimum size of the window
        public static Vector2 SizeMin => new(16, 2);
        // The maximum size of the window
        public static Vector2 SizeMax => new(Console.LargestWindowWidth, Console.LargestWindowHeight);
        // The current size of the window
        public static Vector2 Size => new(Width, Height);
        // The height of the window
        public static int Height => Console.WindowHeight;
        // The width of the window
        public static int Width => Console.WindowWidth;

        #endregion



        #region Universal Variables

        // Initialized as true to avoid drawing during initialization.
        // This should be overridden after initialization is finished.
        public static volatile bool IsDrawing = true;

        #endregion



        #region Universal Methods

        public static void SetSize(Vector2 size) => SetSize(size.x, size.y);

        public static void SetSize(int width, int height)
        {
            // This can only be called on Windows
            if (OperatingSystem.IsWindows())
            {
                Console.SetWindowSize(width, height);
                Console.SetBufferSize(
                    Console.WindowLeft + width,
                    Console.WindowTop + height);
                // This is called twice to fix the scrollbar from showing
                Console.SetWindowSize(width, height);
            }
        }

        public static void Print(object message) => Console.Write(message);

        public static void Print(object message, PrintType printType = PrintType.General) => Print(message, Program.Settings.ColorTheme[printType]);

        public static void Print(object message, ColorPair colors)
        {
            // Override colors if specified
            colors.SetConsoleColors();

            // Print message
            Print(message);

            // Restore old color values
            Program.Settings?.UpdateColors();
        }

        [Obsolete("Use Print(Message, PrintType?) instead.")]
        public static void PrintColor(object message, ConsoleColor? colorText = null, ConsoleColor? colorBG = null)
        {
            // Override colors if specified
            if (colorText.HasValue) Console.ForegroundColor = colorText.Value;
            if (colorBG.HasValue) Console.BackgroundColor = colorBG.Value;

            // Print message
            Console.Write(message);

            // Restore old color values if overriden
            Program.Settings?.UpdateColors();
        }

        // TODO remove this function when all references are gone
        [Obsolete("Use Cursor.SetPosition and Window.Print instead.")]
        public static void PrintLine(object message = null!, ConsoleColor? colorText = null, ConsoleColor? colorBG = null)
        {
            if (message != null)
                Window.PrintColor(message, colorText: colorText, colorBG: colorBG);

            Console.WriteLine();
        }

        public static void Clear() => Console.Clear();

        #endregion
    }
}
