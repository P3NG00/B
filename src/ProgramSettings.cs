using B.Utils;
using B.Utils.Themes;

namespace B
{
    public sealed class ProgramSettings
    {
        public static string Path => Program.DataPath + "settings";

        public Togglable CursorVisible;
        public Togglable DebugMode;
        public Togglable Censor;
        public ColorTheme ColorTheme;
        public int CursorSize;

        public ProgramSettings()
        {
            CursorVisible = new(b => UpdateCursor());
            DebugMode = new(b => Window.Clear());
            Censor = new();
            ColorTheme = Util.ThemeDefault;
            CursorSize = 100;
        }

        public void UpdateColors()
        {
            ColorPair pair = ColorTheme[PrintType.General];
            pair.SetConsoleColors();
        }

        public void UpdateCursor()
        {
            Cursor.Visible = CursorVisible.Active;
            Cursor.Size = CursorSize;
        }
    }
}
