namespace B
{
    public static class ProgramThread
    {
        private static object _lock = new();

        public static void Lock() => Monitor.Enter(_lock);

        public static void Unlock() => Monitor.Exit(_lock);

        public static void Wait(int milliseconds = 10) => Thread.Sleep(milliseconds);

        public static Thread StartLoopedThread(Action action)
        {
            Thread thread = new(() =>
            {
                while (Program.Instance.IsRunning)
                    action();
            });
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }
    }
}
