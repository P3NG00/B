using B.Options;
using B.Utils;

namespace B.Inputs
{
    public sealed class Keybind
    {
        #region Universal Properties

        public static readonly List<Keybind> Keybinds = new();

        #endregion



        #region Public Properties

        public readonly ConsoleKey? Key;
        public readonly char? KeyChar;
        public readonly string Description;
        public readonly Action Action;

        public Vector2 Position { get; private set; } = null!;

        public bool IsHighlighted
        {
            get
            {
                if (Position is null)
                    throw new Exception("Cannot highlight Keybind without a position set!");

                Vector2 mousePos = Mouse.Position;
                bool inLeftEdge = mousePos.x >= Position.x;
                bool inRightEdge = mousePos.x < Position.x + ToString().Length;
                bool isInX = inLeftEdge && inRightEdge;
                bool isOnY = mousePos.y == Position.y;
                return isInX && isOnY;
            }
        }

        public bool Display => !string.IsNullOrWhiteSpace(Description);

        #endregion



        #region Private Properties

        private readonly ConsoleModifiers _modifiers;

        #endregion



        #region Constructor

        private Keybind(Action? action = null, string? description = null, char? keyChar = null, ConsoleKey? key = null, ConsoleModifiers? modifiers = null)
        {
            if (action is null)
                Action = Util.Void;
            else
                Action = action;

            KeyChar = keyChar;
            Key = key;

            if (description is null)
                Description = string.Empty;
            else
                Description = description;

            if (modifiers.HasValue)
                _modifiers = modifiers.Value;
            else
                _modifiers = default;
        }

        #endregion



        #region Public Methods

        public void SetPosition(Vector2 position)
        {
            if (Position is not null)
                throw new Exception("Position already set!");

            Position = position;
        }

        public void Print()
        {
            if (!Display)
                return;

            Cursor.Position = Position;
            string keybindStr = ToString();

            if (IsHighlighted)
                Window.Print(keybindStr, PrintType.Highlight);
            else
                Window.Print(keybindStr, PrintType.General);
        }

        #endregion



        #region Private Methods

        private bool HasModifier(ConsoleModifiers modifier) => _modifiers.HasFlag(modifier);

        #endregion



        #region Override Methods

        public override bool Equals(object? obj)
        {
            if (obj is Keybind keybind)
                return this == keybind;
            else if (obj is ConsoleKeyInfo keyInfo)
                return this == keyInfo;

            return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            string preface = string.Empty;
            if (HasModifier(ConsoleModifiers.Control)) preface += "Ctrl+";
            if (HasModifier(ConsoleModifiers.Shift)) preface += "Shift+";
            if (HasModifier(ConsoleModifiers.Alt)) preface += "Alt+";
            return $"{preface}{(KeyChar.HasValue ? KeyChar.Value.ToString() : Key.ToString())}) {Description}";
        }

        #endregion



        #region Universal Methods

        public static Keybind Create(Action? action = null, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), ConsoleModifiers? modifiers = null)
        {
            Keybind keybind = new Keybind(action, description, keyChar, key, modifiers);
            return keybind;
        }

        public static Keybind CreateOptionExit(IOption option)
        {
            string phrase = string.Empty;

            switch (Program.Instance.Stage)
            {
                case Program.Levels.Program: phrase = "Quit"; break;
                case Program.Levels.Group: phrase = "Back"; break;
                case Program.Levels.Option: phrase = "Exit"; break;
                default: throw new Exception($"Invalid Program Stage: {Program.Instance.Stage}");
            }

            Action exitAction = () => option.Quit();
            Keybind optionExitKeybind = new Keybind(exitAction, phrase, key: ConsoleKey.Escape);
            return optionExitKeybind;
        }

        public static Keybind CreateConfirmation(Action action, string message, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), ConsoleModifiers? modifiers = null)
        {
            Action confirmationAction = () =>
            {
                Window.IsDrawing = true;
                Window.Clear();
                Window.SetSize(message.Length + 6, 7);
                Input.Choice choice = Input.Choice.Create(message);
                choice.AddKeybind(Keybind.Create(description: "NO", key: ConsoleKey.Escape));
                choice.AddSpacer();
                choice.AddKeybind(Keybind.Create(action, "yes", key: ConsoleKey.Enter));
                Cursor.y = 1;
                choice.Request();
                // Clear window after confirmation
                Window.Clear();
            };

            Keybind confirmationKeybind = new Keybind(confirmationAction, description, keyChar, key, modifiers);
            return confirmationKeybind;
        }

        #endregion



        #region Operator Overrides

        public static bool operator ==(Keybind keybind, ConsoleKeyInfo keyInfo)
        {
            bool isKey = keybind.Key == keyInfo.Key;
            bool isKeyChar;
            char? keychar = keybind.KeyChar;

            if (keychar.HasValue)
                isKeyChar = keychar.Value == keyInfo.KeyChar;
            else
                isKeyChar = false;

            bool keyMatches = isKey || isKeyChar;

            if (!keyMatches)
                return false;

            return keybind._modifiers.Equals(keyInfo.Modifiers);
        }

        public static bool operator !=(Keybind keybind, ConsoleKeyInfo keyInfo) => !(keybind == keyInfo);

        #endregion
    }
}
