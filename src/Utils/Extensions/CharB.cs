namespace B.Utils.Extensions
{
    public static class CharB
    {
        public static string Loop(this char c, int times) => new string(c, times);

        public static char Unformat(this char c) => new char[] { (char)7, (char)8, (char)9, (char)10, (char)13 }.Contains(c) ? ' ' : c;
    }
}
