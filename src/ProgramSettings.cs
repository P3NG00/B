using B.Utils;
using B.Utils.Enums;
using B.Utils.Themes;

namespace B
{
    public sealed class ProgramSettings
    {
        #region Universal Properties

        public static string Path => Program.DataPath + "settings";

        #endregion



        #region Public Variables

        public Togglable CursorVisible;
        public Togglable DebugMode;
        public Togglable Censor;
        public ColorTheme ColorTheme;
        public int CursorSize;

        #endregion



        #region Constructor

        public ProgramSettings()
        {
            CursorVisible = new();
            DebugMode = new();
            Censor = new();
            ColorTheme = Util.ThemeDefault;
            CursorSize = 100;
        }

        #endregion



        #region Public Methods

        public void Initialize()
        {
            // These are set outside of constructor because these are not serializable.
            // Since they are not serializable, they need to be re-initialized every time the program is run instead of being saved.
            CursorVisible.SetOnChangeAction(UpdateCursor);
            DebugMode.SetOnChangeAction(Window.Clear);

            UpdateColors();
            UpdateCursor();
        }

        public void UpdateColors() => ColorTheme[PrintType.General].SetConsoleColors();

        public void UpdateCursor()
        {
            Cursor.Visible = CursorVisible;
            Cursor.Size = CursorSize;
        }

        #endregion
    }
}
