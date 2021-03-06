using B.Modules;
using B.Utils;
using B.Utils.Enums;
using B.Utils.Themes;

namespace B.Inputs
{
    public sealed class Keybind
    {
        #region Public Properties

        // The position to begin printing the keybind.
        public Vector2 Position { get; private set; } = null!;
        // Returns if the mouse is currently hovering over the keybind.
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
        // Returns if the keybind should be displayed on screen.
        public bool Display => !string.IsNullOrWhiteSpace(_description);

        #endregion



        #region Private Variables

        // List of currently registered Keybinds.
        private static readonly List<Keybind> _keybinds = new();

        // Required key modifiers.
        // This is a combination of Control, Shift, and Alt.
        private ConsoleModifiers _modifiers;
        // Short description to display.
        private string _description = null!;
        // Action to perform when activated.
        private Action _action = null!;
        // The 
        private ColorPair? _colorPair;
        private ConsoleKey? _key;
        private char? _keyChar;
        // Last highlight state cache
        private bool? _lastHighlighted = null;

        #endregion



        #region Constructors

        // This is here to ensure keybinds are created through the Universal methods
        private Keybind() { }

        #endregion



        #region Public Methods

        // Prints the keybind to the screen.
        public void Print()
        {
            // Display check.
            if (!Display)
                throw new Exception("Attempted to print a keybind that is not displayed!");

            // Get output
            string keybindStr = ToString();
            // Get current highlight status
            bool highlighted = IsHighlighted;
            // Compare to last highlight status
            // Check 'overrides' (reasons to re-print regardless of last state)
            bool overrides = !_lastHighlighted.HasValue || Mouse.KeybindsNeedRedraw;
            if (!highlighted && (overrides || _lastHighlighted!.Value))
            {
                if (_colorPair is null)
                    CacheCursorAndPrint(() => Window.Print(keybindStr));
                else
                    CacheCursorAndPrint(() => Window.Print(keybindStr, _colorPair));
            }
            else if (highlighted && (overrides || !_lastHighlighted!.Value))
                CacheCursorAndPrint(() => Window.Print(keybindStr, PrintType.Highlight));
            // Update last highlight status
            _lastHighlighted = highlighted;

            // Local Method.
            // Since keybinds are printed when the mouse is moving,
            // this method caches the cursor position and visibility
            // before printing to avoid the cursor from flickering.
            void CacheCursorAndPrint(Action printAction)
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

        // Returns if the given modifiers are required.
        private bool HasModifier(ConsoleModifiers modifier) => _modifiers.HasFlag(modifier);

        #endregion



        #region Override Methods

        // Compares a keybinds to an object.
        public override bool Equals(object? obj)
        {
            if (obj is Keybind keybind)
                return this == keybind;
            else if (obj is ConsoleKeyInfo keyInfo)
                return this == keyInfo;
            else
                return false;
        }

        // Returns the hashcode.
        public override int GetHashCode() => base.GetHashCode();

        // The string representation of the keybind.
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

        // Creates a new keybind.
        public static Keybind Create(Action? action = null, string? description = null, char? keyChar = null, ConsoleKey? key = null, ConsoleModifiers? modifiers = null, ColorPair? colorPair = null)
        {
            Keybind keybind = new();
            keybind._description = description ?? string.Empty;
            keybind._modifiers = modifiers ?? default;
            keybind._action = action ?? Util.Void;
            keybind._colorPair = colorPair;
            keybind._keyChar = keyChar;
            keybind._key = key;
            return keybind;
        }

        // Creates a keybind meant to be used to exit the given Module.
        public static Keybind CreateModuleExit(IModule option, bool hide = false)
        {
            string? phrase = null;

            if (!hide)
            {
                switch (Program.Instance.Stage)
                {
                    case Program.Stages.Program: phrase = "Quit"; break;
                    case Program.Stages.Group: phrase = "Back"; break;
                    case Program.Stages.Module: phrase = "Exit"; break;
                    default: throw new Exception($"Invalid Program Stage");
                }
            }

            return Create(option.Quit, phrase, key: ConsoleKey.Escape);
        }

        // Creates a keybind meant to toggle Togglables.
        public static Keybind CreateTogglable(Togglable togglable, string title, char? keyChar = null, ConsoleKey? key = null, ConsoleModifiers? modifiers = null, ColorPair? colorPair = null) => Create(togglable.Toggle, $"{title}: {togglable.Enabled,-5}", keyChar, key, modifiers, colorPair);

        // Creates a keybind that displays a confirmation before performing the given action.
        public static Keybind CreateConfirmation(Action action, string deletionMessage, string? description = null, char? keyChar = null, ConsoleKey? key = null, ConsoleModifiers? modifiers = null, ColorPair? colorPair = null)
        {
            return Create(() =>
            {
                ProgramThread.TryLock();
                ClearRegisteredKeybinds();
                Window.Clear();
                Window.SetSize(deletionMessage.Length + 6, 7);
                Choice choice = new(deletionMessage);
                choice.AddKeybind(Create(description: "NO", key: ConsoleKey.Escape));
                choice.AddSpacer();
                choice.AddKeybind(Create(action, "yes", key: ConsoleKey.Enter));
                Cursor.y = 1;
                choice.Request();
                ProgramThread.TryLock();
                // Clear window after confirmation
                Window.Clear();
            }, description, keyChar, key, modifiers, colorPair);
        }

        // Registers the given keybind.
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

        // Prints currently registered keybinds.
        public static void PrintRegisteredKeybinds()
        {
            foreach (var keybind in _keybinds)
                if (keybind.Display)
                    keybind.Print();
        }

        // Uses the condition to find a matching keybind to activate.
        public static void ActivateKeybind(Func<Keybind, bool> condition)
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

        // Clears currently registered keybinds.
        public static void ClearRegisteredKeybinds()
        {
            _keybinds.Clear();
            Mouse.MarkKeybindsForRedraw();
        }

        #endregion



        #region Operator Overrides

        // Returns if the ConsoleKeyInfo fills the requirements of the Keybind.
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

        // Returns if the ConsoleKeyInfo does not fill the requirements of the Keybind.
        public static bool operator !=(Keybind keybind, ConsoleKeyInfo keyInfo) => !(keybind == keyInfo);

        #endregion
    }
}
