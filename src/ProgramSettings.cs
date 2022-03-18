using B.Utils;

namespace B
{
    public sealed class ProgramSettings
    {
        public static string Path => Program.DataPath + "settings";

        public Togglable CursorVisible;
        public Togglable DebugMode;
        public Togglable Censor;
        public int CursorSize = 100;
        public ConsoleColor ColorBackground = ConsoleColor.White;
        public ConsoleColor ColorText = ConsoleColor.Black;

        public ProgramSettings()
        {
            CursorVisible = new(b => UpdateCursor());
            DebugMode = new(b => Window.Clear());
            Censor = new();
        }

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
