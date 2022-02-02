namespace B.Utils
{
    public sealed class Keybind
    {
        public static readonly Keybind NULL = new(null!);

        public readonly ConsoleKey Key;
        public readonly char KeyChar;
        public readonly string Description;
        public readonly Action Action;

        // TODO change from ConsoleKey to ConsoleKeyInfo OR add ConsoleKeyModifiers
        public Keybind(Action action, string description = Util.NULL_STRING, char keyChar = Util.NULL_CHAR, ConsoleKey key = default(ConsoleKey))
        {
            this.KeyChar = keyChar;
            this.Key = key;
            this.Description = description;
            this.Action = action;
        }
    }
}
