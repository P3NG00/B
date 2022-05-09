using B.Utils.Enums;
using B.Utils.Themes;

namespace B.Utils
{
    public static class Util
    {
        #region Private Variables

        private static ColorTheme[] _colorThemes = null!;

        #endregion



        #region Universal Properties

        public static Random Random { get; private set; } = new();
        public static ColorTheme[] ColorThemes => _colorThemes;
        public static ColorTheme ThemeDefault => new ColorTheme("Default",
            new PrintPair(PrintType.General, new(ConsoleColor.Black, ConsoleColor.White)),
            new PrintPair(PrintType.Title, new(ConsoleColor.Black, ConsoleColor.Gray)));
        public static ConsoleColor[] ConsoleColors => new ConsoleColor[]
        {
            ConsoleColor.White,
            ConsoleColor.Gray,
            ConsoleColor.DarkGray,
            ConsoleColor.Black,
            ConsoleColor.DarkMagenta,
            ConsoleColor.Magenta,
            ConsoleColor.DarkBlue,
            ConsoleColor.Blue,
            ConsoleColor.DarkCyan,
            ConsoleColor.Cyan,
            ConsoleColor.DarkGreen,
            ConsoleColor.Green,
            ConsoleColor.DarkYellow,
            ConsoleColor.Yellow,
            ConsoleColor.DarkRed,
            ConsoleColor.Red,
        };

        #endregion



        #region Universal Methods

        public static void Void() { }

        public static void WaitFor(Func<bool> condition)
        {
            while (!condition())
                ProgramThread.Wait();
        }

        public static void WaitFor(Func<bool> condition, float seconds)
        {
            while (!condition())
                ProgramThread.Wait(seconds);
        }

        public static void Initialize()
        {
            // Init check
            if (_colorThemes is not null)
                throw new Exception("Util already initialized!");

            // When creating color themes:
            // PrintType.General MUST be defined or exception will be thrown.
            // If PrintType.Highlight is not defined, it will be created automatically as an inverted version of PrintType.General.
            // All other PrintTypes are optional.
            _colorThemes = new ColorTheme[]
            {
                ThemeDefault,
                new("Default Red",
                    new PrintPair(PrintType.General, new(ConsoleColor.DarkRed, ConsoleColor.White)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.Red, ConsoleColor.Gray))),
                new("Default Green",
                    new PrintPair(PrintType.General, new(ConsoleColor.Green, ConsoleColor.White)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.DarkGreen, ConsoleColor.Gray))),
                new("Default Blue",
                    new PrintPair(PrintType.General, new(ConsoleColor.DarkCyan, ConsoleColor.White)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.Blue, ConsoleColor.Gray))),
                new("Default Faded",
                    new PrintPair(PrintType.General, new(ConsoleColor.DarkGray, ConsoleColor.White)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.DarkGray, ConsoleColor.Gray))),
                new("Default Console",
                    new PrintPair(PrintType.General, new(ConsoleColor.White, ConsoleColor.Black)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.Black, ConsoleColor.White))),
                new("Sky",
                    new PrintPair(PrintType.General, new(ConsoleColor.White, ConsoleColor.DarkCyan)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.DarkCyan, ConsoleColor.White))),
                new("Sunshine",
                    new PrintPair(PrintType.General, new(ConsoleColor.DarkYellow, ConsoleColor.Yellow)),
                    new PrintPair(PrintType.Highlight, new(ConsoleColor.DarkYellow, ConsoleColor.White))),
                new("Salmon",
                    new PrintPair(PrintType.General, new(ConsoleColor.Yellow, ConsoleColor.Red)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.Yellow, ConsoleColor.DarkRed))),
                new("Creeper",
                    new PrintPair(PrintType.General, new(ConsoleColor.Black, ConsoleColor.Green)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.Black, ConsoleColor.White))),
                new("Streetlight",
                    new PrintPair(PrintType.General, new(ConsoleColor.Yellow, ConsoleColor.Black)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.Yellow, ConsoleColor.DarkGray))),
                new("Hacker",
                    new PrintPair(PrintType.General, new(ConsoleColor.Green, ConsoleColor.Black)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.Black, ConsoleColor.Green))),
            };
        }

        #endregion
    }
}
