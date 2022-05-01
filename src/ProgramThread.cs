namespace B
{
    public static class ProgramThread
    {
        #region Private Variables

        // This object is used for thread-locking.
        private static object _lock = new();

        #endregion



        #region Universal Properties

        public static object LockObject => _lock;

        #endregion



        #region Private Properties

        private static bool Locked => Monitor.IsEntered(_lock);

        #endregion



        #region Universal Methods

        public static void Lock(Action action)
        {
            lock (_lock)
                action();
        }

        public static void TryLock()
        {
            if (!Locked)
                Monitor.Enter(_lock);
        }

        public static void TryUnlock()
        {
            if (Locked)
                Monitor.Exit(_lock);
        }

        public static void Wait(float seconds = 0.01f)
        {
            int milliseconds = (int)(seconds * 1000f);
            Thread.Sleep(milliseconds);
        }

        public static Thread StartLoopedThread(string name, Action action, ThreadPriority priority)
        {
            Thread thread = new(() =>
            {
                while (Program.Instance.IsRunning)
                    action();
            });
            thread.Name = name;
            thread.Priority = priority;
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }

        #endregion
    }
}
