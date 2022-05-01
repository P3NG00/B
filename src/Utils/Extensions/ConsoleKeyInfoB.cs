namespace B.Utils.Extensions
{
    public static class ConsoleKeyInfoB
    {
        public static bool IsControlHeld(this ConsoleKeyInfo keyInfo) => keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control);

        public static bool IsShiftHeld(this ConsoleKeyInfo keyInfo) => keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift);

        public static bool IsAltHeld(this ConsoleKeyInfo keyInfo) => keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt);
    }
}
