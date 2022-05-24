namespace B.Utils.Extensions
{
    public static class CharB
    {
        // Repeats a char a number of times.
        public static string Loop(this char c, int repeat) => new string(c, repeat);

        // Replaces chars that mess with text layout with a space char.
        public static char Unformat(this char c) => new char[] { (char)7, (char)8, (char)9, (char)10, (char)13 }.Contains(c) ? ' ' : c;
    }
}
