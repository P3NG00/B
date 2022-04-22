using B.Utils.Themes;

namespace B.Utils
{
    public static class Util
    {
        // Private
        private static readonly Random _random = new Random();

        private static ColorTheme[] _colorThemes = null!;

        // Universal Properties
        public static ColorTheme ThemeDefault => new ColorTheme("Default",
            new PrintPair(PrintType.General, new(ConsoleColor.Black, ConsoleColor.White)),
            new PrintPair(PrintType.Title, new(ConsoleColor.Black, ConsoleColor.Gray)));

        // Universal Getters
        public static ColorTheme[] ColorThemes => _colorThemes;
        public static Random Random => _random;

        // Universal Functions
        public static void Initialize()
        {
            if (_colorThemes is not null)
                throw new Exception("Util already initialized!");

            _colorThemes = new ColorTheme[]
            {
                ThemeDefault,
                new("Light",
                    new PrintPair(PrintType.General, new(ConsoleColor.DarkGray, ConsoleColor.White)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.DarkGray, ConsoleColor.Gray))),
                new("Gentle",
                    new PrintPair(PrintType.General, new(ConsoleColor.DarkCyan, ConsoleColor.White)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.Blue, ConsoleColor.Gray))),
                new("Sky",
                    new PrintPair(PrintType.General, new(ConsoleColor.White, ConsoleColor.DarkCyan)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.DarkCyan, ConsoleColor.White))),
                new("Sunshine",
                    new PrintPair(PrintType.General, new(ConsoleColor.DarkYellow, ConsoleColor.Yellow))),
                new("Streetlight",
                    new PrintPair(PrintType.General, new(ConsoleColor.Yellow, ConsoleColor.Black)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.Yellow, ConsoleColor.DarkGray))),
                new("Salmon",
                    new PrintPair(PrintType.General, new(ConsoleColor.Yellow, ConsoleColor.Red)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.Yellow, ConsoleColor.DarkRed))),
                new("Console",
                    new PrintPair(PrintType.General, new(ConsoleColor.White, ConsoleColor.Black))),
                new("Creeper",
                    new PrintPair(PrintType.General, new(ConsoleColor.Black, ConsoleColor.Green)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.Black, ConsoleColor.White))),
                new("Hacker",
                    new PrintPair(PrintType.General, new(ConsoleColor.Green, ConsoleColor.Black)),
                    new PrintPair(PrintType.Title, new(ConsoleColor.Black, ConsoleColor.Green))),
            };
        }

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

        public static void Void() { }

        public static void Loop(int count, Action action) => Loop(count, i => action());

        public static void Loop(int count, Action<int> actionIndexed)
        {
            for (int i = 0; i < count; i++)
                actionIndexed(i);
        }

        public static void WaitFor(Func<bool> condition)
        {
            while (!condition())
                Thread.Sleep(5);
        }

        public static void StartLoopedThread(Action action, out Thread thread)
        {
            thread = new(() =>
            {
                while (Program.Instance.IsRunning)
                    action();
            });
            thread.IsBackground = true;
            thread.Start();
        }
    }
}
