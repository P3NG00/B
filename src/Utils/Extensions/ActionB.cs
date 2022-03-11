namespace B.Utils.Extensions
{
    public static class ActionB
    {
        public static void Loop(this Action action, int times)
        {
            for (int i = 0; i < times; i++)
                action();
        }

        // The int in the action is the current loop number
        public static void Loop(this Action<int> action, int times)
        {
            for (int i = 0; i < times; i++)
                action(i);
        }
    }
}
