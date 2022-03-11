namespace B.Utils.Extensions
{
    public static class StringB
    {
        public static string Loop(this string str, int times)
        {
            string s = string.Empty;
            Action action = () => s += str;
            action.Loop(times);
            return s;
        }
    }
}
