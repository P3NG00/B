namespace B
{
    public static class ProgramThread
    {
        private static object _lock = new();

        private static bool Locked => Monitor.IsEntered(_lock);

        public static void Lock()
        {
            if (!Locked)
                Monitor.Enter(_lock);
        }

        public static void Unlock()
        {
            if (Locked)
                Monitor.Exit(_lock);
        }

        public static void Wait(int milliseconds = 10) => Thread.Sleep(milliseconds);

        public static Thread StartLoopedThread(string name, Action action)
        {
            Thread thread = new(() =>
            {
                while (Program.Instance.IsRunning)
                    action();
            });
            thread.Name = name;
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }
    }
}
