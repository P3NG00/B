using B.Options;
using B.Utils;

namespace B.Inputs
{
    public sealed class Keybind
    {
        public readonly ConsoleKey Key;
        public readonly char? KeyChar;
        public readonly string? Description;
        public readonly Action Action;
        private readonly ConsoleModifiers _modifiers;

        public Keybind(Action action, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), ConsoleModifiers? modifiers = null)
        {
            KeyChar = keyChar;
            Key = key;
            Description = description;
            Action = action;

            if (modifiers.HasValue)
                _modifiers = modifiers.Value;
            else
                _modifiers = default;
        }

        public bool HasModifier(ConsoleModifiers modifier) => _modifiers.HasFlag(modifier);

        private bool IsValid(ConsoleKeyInfo keyInfo)
        {
            if (Action is null)
                return false;

            bool isKey = Key == keyInfo.Key;
            bool isKeyChar;

            if (KeyChar.HasValue)
                isKeyChar = KeyChar.Value == keyInfo.KeyChar;
            else
                isKeyChar = false;

            bool keyMatches = isKey || isKeyChar;

            if (!keyMatches)
                return false;

            return _modifiers.Equals(keyInfo.Modifiers);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Keybind keybind)
                return this == keybind;
            else if (obj is ConsoleKeyInfo keyInfo)
                return IsValid(keyInfo);
            else
                return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            // If no description...
            if (string.IsNullOrWhiteSpace(Description))
            {
                // If action is null...
                if (Action is null)
                {
                    // Treat as spacer
                    return string.Empty;
                }
                else
                {
                    // This keybind is supposed to be hidden.
                    // Do not display it and do not move to next line.
                    return null!;
                }
            }
            // If description exists...
            else
            {
                // If action is null, proceed as message
                if (Action is null)
                    return Description;
                // If action is not null, proceed as keybind
                else
                {
                    string preface = string.Empty;
                    if (HasModifier(ConsoleModifiers.Control)) preface += "Ctrl+";
                    if (HasModifier(ConsoleModifiers.Shift)) preface += "Shift+";
                    if (HasModifier(ConsoleModifiers.Alt)) preface += "Alt+";
                    return $"{preface}{(KeyChar.HasValue ? KeyChar.Value.ToString() : Key.ToString())}) {Description}";
                }
            }
        }

        public static Keybind CreateOptionExitKeybind(IOption option)
        {
            string phrase = string.Empty;

            switch (Program.Instance.Stage)
            {
                case Program.Levels.Program: phrase = "Quit"; break;
                case Program.Levels.Group: phrase = "Back"; break;
                case Program.Levels.Option: phrase = "Exit"; break;
            }

            return new(() => option.Quit(), phrase, key: ConsoleKey.Escape);
        }

        public static Keybind CreateConfirmationKeybind(Action action, string message, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), ConsoleModifiers? modifiers = null) => new(() =>
        {
            Window.Clear();
            Window.Size = new(message.Length + 4, 7);
            Cursor.Position = new(2, 1);
            Input.Choice choice = Input.Choice.Create(message);
            choice.Add(Util.Void, "NO", key: ConsoleKey.Escape);
            choice.AddSpacer();
            choice.Add(action, "yes", key: ConsoleKey.Enter);
            choice.Request();
            Window.Clear();
        }, description, keyChar, key, modifiers);

        public static bool operator ==(Keybind keybind, ConsoleKeyInfo keyInfo) => keybind.IsValid(keyInfo);

        public static bool operator !=(Keybind keybind, ConsoleKeyInfo keyInfo) => !keybind.IsValid(keyInfo);
    }
}
