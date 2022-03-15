using B.Utils;

namespace B
{
    public sealed class ProgramSettings
    {
        public static string Path => Program.DataPath + "settings";

        public Togglable CursorVisible = new();
        public Togglable DebugMode = new();
        public Togglable Censor = new();
        public int CursorSize = 100;
        public ConsoleColor ColorBackground = ConsoleColor.White;
        public ConsoleColor ColorText = ConsoleColor.Black;

        public void UpdateAll()
        {
            UpdateColors();
            UpdateCursor();
        }

        public void UpdateColors()
        {
            Console.BackgroundColor = ColorBackground;
            Console.ForegroundColor = ColorText;
        }

        public void UpdateCursor()
        {
            Cursor.Visible = CursorVisible.Active;
            Cursor.Size = CursorSize;
        }
    }
}
