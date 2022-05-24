using B.Utils;
using B.Utils.Enums;
using B.Utils.Themes;

namespace B
{
    // Stores settings for the program.
    public sealed class ProgramSettings
    {
        #region Universal Properties

        // Relative path to serialized program settings file.
        public static string Path => Program.DataPath + "settings";

        #endregion



        #region Public Variables

        // Togglable to display goodbye screen when exiting.
        public Togglable DisplayGoodbye;
        // Togglable to display the cursor on screen.
        public Togglable CursorVisible;
        // Togglable for Debug mode.
        public Togglable DebugMode;
        // Togglable to censor bad language.
        public Togglable Censor;
        // Selected color theme.
        public ColorTheme ColorTheme;
        // Size of cursor when visible (1-100).
        public int CursorSize;

        #endregion



        #region Constructor

        // Creates new ProgramSettings object with default values.
        public ProgramSettings()
        {
            DisplayGoodbye = new(true);
            CursorVisible = new(false);
            DebugMode = new(false);
            Censor = new(false);
            ColorTheme = Util.ThemeDefault;
            CursorSize = 100;
        }

        #endregion



        #region Public Methods

        // Since actions cannot be saved in a serialized file, this
        // function appropriately reinitializes togglables with actions.
        public void Initialize()
        {
            // These are set outside of constructor because these are not serializable.
            // Since they are not serializable, they need to be re-initialized every time the program is run instead of being saved.
            CursorVisible.SetOnChangeAction(UpdateCursor);
            DebugMode.SetOnChangeAction(Window.Clear);
            // Update
            UpdateColors();
            UpdateCursor();
        }

        // Used to reset the color theme after colors have been changed.
        public void UpdateColors() => ColorTheme[PrintType.General].SetConsoleColors();

        // Used to reset the cursor after properties have been changed.
        public void UpdateCursor()
        {
            Cursor.Visible = CursorVisible;
            Cursor.Size = CursorSize;
        }

        #endregion
    }
}
