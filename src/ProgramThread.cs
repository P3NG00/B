namespace B
{
    public static class ProgramThread
    {
        private static object _lock = new();

        public static void Lock() => Monitor.Enter(_lock);

        public static void Unlock()
        {
            if (Monitor.IsEntered(_lock))
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
