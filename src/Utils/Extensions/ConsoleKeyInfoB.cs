namespace B.Utils.Extensions
{
    public static class ConsoleKeyInfoB
    {
        // Checks if ConsoleKeyInfo contains ConsoleModifiers.Control.
        public static bool IsControlHeld(this ConsoleKeyInfo keyInfo) => keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control);

        // Checks if ConsoleKeyInfo contains ConsoleModifiers.Shift.
        public static bool IsShiftHeld(this ConsoleKeyInfo keyInfo) => keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift);

        // Checks if ConsoleKeyInfo contains ConsoleModifiers.Alt.
        public static bool IsAltHeld(this ConsoleKeyInfo keyInfo) => keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt);
    }
}
