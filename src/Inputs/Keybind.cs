namespace B.Inputs
{
    public sealed class Keybind
    {
        public readonly ConsoleKey Key;
        public readonly char? KeyChar;
        public readonly string? Description;
        public readonly Action Action;
        private readonly ConsoleModifiers _modifiers;

        public bool ModifierControl => _modifiers.HasFlag(ConsoleModifiers.Control);
        public bool ModifierShift => _modifiers.HasFlag(ConsoleModifiers.Shift);
        public bool ModifierAlt => _modifiers.HasFlag(ConsoleModifiers.Alt);

        public Keybind(Action action, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), bool control = false, bool shift = false, bool alt = false)
        {
            KeyChar = keyChar;
            Key = key;
            Description = description;
            Action = action;
            _modifiers = 0;
            if (control) _modifiers |= ConsoleModifiers.Control;
            if (shift) _modifiers |= ConsoleModifiers.Shift;
            if (alt) _modifiers |= ConsoleModifiers.Alt;
        }

        private bool IsValid(ConsoleKeyInfo keyInfo) => (Key == keyInfo.Key || (KeyChar.HasValue && KeyChar.Value == keyInfo.KeyChar)) && Action != null && _modifiers == keyInfo.Modifiers;

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

        public static bool operator ==(Keybind keybind, ConsoleKeyInfo keyInfo) => keybind.IsValid(keyInfo);

        public static bool operator !=(Keybind keybind, ConsoleKeyInfo keyInfo) => !keybind.IsValid(keyInfo);
    }
}
