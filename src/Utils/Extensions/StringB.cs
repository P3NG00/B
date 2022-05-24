namespace B.Utils.Extensions
{
    public static class StringB
    {
        // Repeats a string a number of times.
        public static string Loop(this string str, int repeat)
        {
            if (repeat < 0)
                throw new ArgumentOutOfRangeException(nameof(repeat), "Must be greater than or equal to 0.");

            string s = string.Empty;
            for (int i = 0; i < repeat; i++)
                s += str;
            return s;
        }
    }
}
