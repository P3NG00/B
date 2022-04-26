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
            Action = action ?? Util.Void;
            KeyChar = keyChar;
            Key = key;
            Description = description ?? string.Empty;
            _modifiers = modifiers ?? default;
        }

        #endregion



        #region Public Methods

        public void Print()
        {
            // Set cursor position
            Cursor.Position = Position;
            // Get output
            string keybindStr = ToString();
            // Print & highlight
            Window.Print(keybindStr, IsHighlighted ? PrintType.Highlight : PrintType.General);
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
            else
                return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            string preface;

            if (KeyChar.HasValue)
                preface = KeyChar.Value.ToString();
            else if (Key.HasValue)
                preface = Key.Value.ToString();
            else
                preface = string.Empty;

            if (HasModifier(ConsoleModifiers.Alt)) preface = "Alt+" + preface;
            if (HasModifier(ConsoleModifiers.Shift)) preface = "Shift+" + preface;
            if (HasModifier(ConsoleModifiers.Control)) preface = "Ctrl+" + preface;

            if (!string.IsNullOrWhiteSpace(preface))
                preface += ") ";

            string fullString = preface + Description;
            return fullString;
        }

        #endregion



        #region Universal Methods

        public static Keybind Create(Action? action = null, string? description = null, char? keyChar = null, ConsoleKey? key = null, ConsoleModifiers? modifiers = null) => new Keybind(action, description, keyChar, key, modifiers);

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

            return new Keybind(option.Quit, phrase, key: ConsoleKey.Escape);
        }

        public static Keybind CreateConfirmation(Action action, string message, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), ConsoleModifiers? modifiers = null)
        {
            Action confirmationAction = () =>
            {
                Window.Clear();
                Window.SetSize(message.Length + 6, 7);
                var choice = Input.Choice.Create(message);
                choice.AddKeybind(Keybind.Create(description: "NO", key: ConsoleKey.Escape));
                choice.AddSpacer();
                choice.AddKeybind(Keybind.Create(action, "yes", key: ConsoleKey.Enter));
                Cursor.y = 1;
                choice.Request();
                // Clear window after confirmation
                Window.Clear();
            };

            return new Keybind(confirmationAction, description, keyChar, key, modifiers);
        }

        public static void RegisterKeybind(Keybind keybind, Vector2 position = null!)
        {
            // Register check
            if (keybind.Position is not null)
                throw new Exception("Keybind already has a position!");

            // Default position if not specified
            if (position is null)
                position = Cursor.Position;

            // Register keybind
            keybind.Position = position;
            Keybinds.Add(keybind);
        }

        public static void PrintRegisteredKeybinds()
        {
            Keybinds.ForEach(keybind =>
            {
                if (keybind.Display)
                    keybind.Print();
            });
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
