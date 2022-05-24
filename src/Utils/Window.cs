using B.Utils.Enums;
using B.Utils.Themes;

namespace B.Utils
{
    public static class Window
    {
        #region Universal Properties

        // Used to manage and retrieve the size of the window.
        public static Vector2 Size
        {
            get => new(Width, Height);
            set => SetSize(value.x, value.y);
        }
        // Minimum size the window can be set to.
        public static Vector2 SizeMin => new(16, 2);
        // Maximum size the window can be set to, relative to the screen size.
        public static Vector2 SizeMax => new(Console.LargestWindowWidth, Console.LargestWindowHeight);
        // Size of one char in the console.
        public static Vector2 CharSize => new(8, 16);
        // Current height of the window.
        public static int Height => Console.WindowHeight;
        // Current width of the window.
        public static int Width => Console.WindowWidth;

        #endregion



        #region Universal Methods

        // Sets size of the console window.
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

        // Prints text at current cursor position.
        public static void Print(object message) => Console.Write(message);

        // Prints text at current cursor position in optional PrintType.
        public static void Print(object message, PrintType printType = PrintType.General) => Print(message, Program.Settings.ColorTheme[printType]);

        // Prints text at current cursor position in specified ColorPair.
        public static void Print(object message, ColorPair colors)
        {
            // Override colors if specified
            colors.SetConsoleColors();

            // Print message
            Print(message);

            // Restore old color values
            Program.Settings?.UpdateColors();
        }

        // Clears console window of all text.
        public static void Clear() => Console.Clear();

        #endregion
    }
}
