namespace B.Inputs
{
    public sealed class Keybind
    {
        public readonly ConsoleKey Key;
        public readonly char? KeyChar;
        public readonly string? Description;
        public readonly Action Action;
        public readonly bool Control;
        public readonly bool Shift;
        public readonly bool Alt;

        public Keybind(Action action, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey), bool control = false, bool shift = false, bool alt = false)
        {
            this.KeyChar = keyChar;
            this.Key = key;
            this.Description = description;
            this.Action = action;
            this.Control = control;
            this.Shift = shift;
            this.Alt = alt;
        }

        public bool IsValid(ConsoleKeyInfo keyInfo) =>
            (this.Key == keyInfo.Key || (this.KeyChar.HasValue && this.KeyChar.Value == keyInfo.KeyChar)) &&
            this.Action != null &&
            this.Control == keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) &&
            this.Shift == keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift) &&
            this.Alt == keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt);
    }
}
