namespace B.Inputs
{
    public sealed class Keybind
    {
        public readonly ConsoleKey Key;
        public readonly char? KeyChar;
        public readonly string? Description;
        public readonly Action Action;
        private readonly ConsoleModifiers _modifiers;

        public bool ModifierControl => this._modifiers.HasFlag(ConsoleModifiers.Control);
        public bool ModifierShift => this._modifiers.HasFlag(ConsoleModifiers.Shift);
        public bool ModifierAlt => this._modifiers.HasFlag(ConsoleModifiers.Alt);

        public Keybind(Action action, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), bool control = false, bool shift = false, bool alt = false)
        {
            this.KeyChar = keyChar;
            this.Key = key;
            this.Description = description;
            this.Action = action;
            this._modifiers = 0;
            if (control) this._modifiers |= ConsoleModifiers.Control;
            if (shift) this._modifiers |= ConsoleModifiers.Shift;
            if (alt) this._modifiers |= ConsoleModifiers.Alt;
        }

        private bool IsValid(ConsoleKeyInfo keyInfo) => (this.Key == keyInfo.Key || (this.KeyChar.HasValue && this.KeyChar.Value == keyInfo.KeyChar)) && this.Action != null && this._modifiers == keyInfo.Modifiers;

        public override bool Equals(object? obj)
        {
            if (obj is Keybind keybind)
                return this == keybind;
            else if (obj is ConsoleKeyInfo keyInfo)
                return this.IsValid(keyInfo);
            else
                return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public static bool operator ==(Keybind keybind, ConsoleKeyInfo keyInfo) => keybind.IsValid(keyInfo);

        public static bool operator !=(Keybind keybind, ConsoleKeyInfo keyInfo) => !keybind.IsValid(keyInfo);
    }
}
