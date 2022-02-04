namespace B.Inputs
{
    public sealed class Keybind
    {
        public readonly ConsoleKey Key;
        public readonly char? KeyChar;
        public readonly string? Description;
        public readonly Action Action;

        // TODO change from ConsoleKey to ConsoleKeyInfo OR add ConsoleKeyModifiers
        public Keybind(Action action, string? description = null, char? keyChar = null, ConsoleKey key = default(ConsoleKey))
        {
            this.KeyChar = keyChar;
            this.Key = key;
            this.Description = description;
            this.Action = action;
        }
    }
}
