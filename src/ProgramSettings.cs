using B.Utils;

namespace B
{
    public sealed class ProgramSettings
    {
        public static string Path => Program.DataPath + "settings";

        public Togglable DebugMode = new();
        public Togglable Censor = new();
        public ConsoleColor ColorBackground = ConsoleColor.White;
        public ConsoleColor ColorText = ConsoleColor.Black;

        public void UpdateColors()
        {
            Console.BackgroundColor = this.ColorBackground;
            Console.ForegroundColor = this.ColorText;
        }
    }
}
