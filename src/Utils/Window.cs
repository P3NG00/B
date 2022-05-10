using B.Utils.Enums;
using B.Utils.Themes;

namespace B.Utils
{
    public static class Window
    {
        // TODO make 'Window.Update()' method to replace 'Input.Action = Util.Void' calls

        #region Universal Properties

        // The current size of the window
        public static Vector2 Size
        {
            get => new(Width, Height);
            set => SetSize(value.x, value.y);
        }
        // The minimum size of the window
        public static Vector2 SizeMin => new(16, 2);
        // The maximum size of the window
        public static Vector2 SizeMax => new(Console.LargestWindowWidth, Console.LargestWindowHeight);
        // This represents the size of one char in the console
        public static Vector2 CharSize => new(8, 16);
        // The height of the window
        public static int Height => Console.WindowHeight;
        // The width of the window
        public static int Width => Console.WindowWidth;

        #endregion



        #region Universal Methods

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

        public static void Clear() => Console.Clear();

        #endregion
    }
}
