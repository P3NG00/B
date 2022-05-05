using B.Options;
using B.Utils;
using B.Utils.Enums;
using B.Utils.Themes;

namespace B.Inputs
{
    public sealed class Keybind
    {
        #region Public Properties

        public Vector2 Position { get; private set; } = null!;

        public bool IsHighlighted
        {
            get
            {
                if (!Display)
                    return false;

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

        public bool Display => !string.IsNullOrWhiteSpace(_description);

        #endregion



        #region Private Variables

        private static readonly List<Keybind> _keybinds = new();

        private ConsoleKey? _key;
        private char? _keyChar;
        private string _description = null!;
        private Action _action = null!;
        private ColorPair? _colorPair;
        private ConsoleModifiers _modifiers;
        // This is used to print the keybind only when necessary
        private bool? _lastHighlighted = null;

        #endregion



        #region Constructors

        // This is here to ensure keybinds are created through the Universal methods
        private Keybind() { }

        #endregion



        #region Public Methods

        public void Print()
        {
            // Get output
            string keybindStr = ToString();
            // Get current highlight status
            bool highlighted = IsHighlighted;
            // Compare to last highlight status
            if (!_lastHighlighted.HasValue || (!highlighted && _lastHighlighted.Value))
            {
                if (_colorPair is null)
                    PrintAction(() => Window.Print(keybindStr));
                else
                    PrintAction(() => Window.Print(keybindStr, _colorPair));
            }
            else if (highlighted && !_lastHighlighted.Value)
                PrintAction(() => Window.Print(keybindStr, PrintType.Highlight));
            // Update last highlight status
            _lastHighlighted = highlighted;

            // Local Method
            void PrintAction(Action printAction)
            {
                // Cache cursor info
                Vector2 cursorPos = Cursor.Position;
                bool cursorVisible = Cursor.Visible;
                // Modify cursor
                Cursor.Visible = false;
                Cursor.Position = Position;
                // Print action
                printAction();
                // Restore cursor
                Cursor.Position = cursorPos;
                Cursor.Visible = cursorVisible;
            }
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

            if (_keyChar.HasValue)
                preface = _keyChar.Value.ToString();
            else if (_key.HasValue)
                preface = _key.Value.ToString();
            else
                preface = string.Empty;

            if (HasModifier(ConsoleModifiers.Alt))
                preface = "Alt+" + preface;
            if (HasModifier(ConsoleModifiers.Shift))
                preface = "Shift+" + preface;
            if (HasModifier(ConsoleModifiers.Control))
                preface = "Ctrl+" + preface;
            if (!string.IsNullOrWhiteSpace(preface))
                preface += ") ";

            return preface + _description;
        }

        #endregion



        #region Universal Methods

        public static Keybind Create(Action? action = null, string? description = null, char? keyChar = null, ConsoleKey? key = null, ConsoleModifiers? modifiers = null, ColorPair? colorPair = null)
        {
            Keybind keybind = new();
            keybind._action = action ?? Util.Void;
            keybind._keyChar = keyChar;
            keybind._key = key;
            keybind._description = description ?? string.Empty;
            keybind._modifiers = modifiers ?? default;
            keybind._colorPair = colorPair;
            return keybind;
        }

        public static Keybind CreateOptionExit(IOption option)
        {
            string phrase = string.Empty;

            switch (Program.Instance.Stage)
            {
                case Program.Stages.Program: phrase = "Quit"; break;
                case Program.Stages.Group: phrase = "Back"; break;
                case Program.Stages.Option: phrase = "Exit"; break;
                default: throw new Exception($"Invalid Program Stage: {Program.Instance.Stage}");
            }

            return Create(option.Quit, phrase, key: ConsoleKey.Escape);
        }

        public static Keybind CreateConfirmation(Action action, string message, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), ConsoleModifiers? modifiers = null, ColorPair? colorPair = null)
        {
            Action confirmationAction = () =>
            {
                ProgramThread.TryLock();
                ClearRegisteredKeybinds();
                Window.Clear();
                Window.SetSize(message.Length + 6, 7);
                Choice choice = new(message);
                choice.AddKeybind(Create(description: "NO", key: ConsoleKey.Escape));
                choice.AddSpacer();
                choice.AddKeybind(Create(action, "yes", key: ConsoleKey.Enter));
                Cursor.y = 1;
                choice.Request();
                ProgramThread.TryLock();
                // Clear window after confirmation
                Window.Clear();
            };

            return Create(confirmationAction, description, keyChar, key, modifiers, colorPair);
        }

        public static void RegisterKeybind(Keybind keybind, Vector2 position = null!)
        {
            // Register check
            if (_keybinds.Contains(keybind))
                throw new Exception("Keybind already registered!");
            // Use cursor position if not specified
            if (position is null)
                position = Cursor.Position;
            // Set keybind position
            keybind.Position = position;
            // Add keybind
            _keybinds.Add(keybind);
        }

        public static void PrintRegisteredKeybinds()
        {
            _keybinds.ForEach(keybind =>
            {
                if (keybind.Display)
                    keybind.Print();
            });
        }

        public static void FindKeybind(Func<Keybind, bool> condition)
        {
            foreach (Keybind keybind in _keybinds)
            {
                // If keybind satisfies condition
                if (condition(keybind))
                {
                    // Set action to be triggered
                    Input.Action = keybind._action;
                    // Finish method
                    return;
                }
            }
        }

        public static void ClearRegisteredKeybinds() => _keybinds.Clear();

        #endregion



        #region Operator Overrides

        public static bool operator ==(Keybind keybind, ConsoleKeyInfo keyInfo)
        {
            bool isKey = keybind._key == keyInfo.Key;
            bool isKeyChar;
            char? keychar = keybind._keyChar;

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
