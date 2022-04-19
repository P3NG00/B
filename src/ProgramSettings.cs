using B.Utils;
using B.Utils.Themes;

namespace B
{
    public sealed class ProgramSettings
    {
        public static string Path => Program.DataPath + "settings";

        // TODO Figure out why Togglables arent being deserialized properly
        // they are saving to the file but not being loaded properly
        // TODO USE DEBUG MODE UPON DESERIALIZATION OF THESE PROPERTIES TO SEE WHATS HAPPENING
        public Togglable CursorVisible;
        public Togglable DebugMode;
        public Togglable Censor;
        public ColorTheme ColorTheme;
        public int CursorSize;

        public ProgramSettings()
        {
            CursorVisible = new();
            DebugMode = new();
            Censor = new();
            ColorTheme = Util.ThemeDefault;
            CursorSize = 100;
        }

        public void Initialize()
        {
            // These are set outside of constructor because these are not serializable.
            // Since they are not serializable, they need to be re-initialized every time the program is run instead of being saved.
            CursorVisible.SetOnChangeAction(b => UpdateCursor());
            DebugMode.SetOnChangeAction(b => Window.Clear());

            UpdateColors();
            UpdateCursor();
        }

        public void UpdateColors() => ColorTheme[PrintType.General].SetConsoleColors();

        public void UpdateCursor()
        {
            Cursor.Visible = CursorVisible.Active;
            Cursor.Size = CursorSize;
        }
    }
}
