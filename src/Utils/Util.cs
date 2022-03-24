namespace B.Utils
{
    public static class Util
    {
        public static Action Void => () => { };

        public static Random Random => new Random();

        public static ConsoleColor[] ConsoleColors => new ConsoleColor[] {
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
            ConsoleColor.Red
        };

        public static void Loop(int count, Action action)
        {
            for (int i = 0; i < count; i++)
                action();
        }

        public static void Loop(int count, Action<int> actionIndexed)
        {
            for (int i = 0; i < count; i++)
                actionIndexed(i);
        }
    }
}
