namespace B.Utils.Extensions
{
    public static class StringB
    {
        public static string Loop(this string str, int times)
        {
            string s = string.Empty;
            for (int i = 0; i < times; i++)
                s += str;
            return s;
        }

        public static bool EqualsIgnoreCase(this string str, string other) => str.Equals(other, StringComparison.OrdinalIgnoreCase);
    }
}
