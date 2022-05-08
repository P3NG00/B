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

        public static Thread StartThread(string name, Action action, ThreadPriority priority = ThreadPriority.Normal)
        {
            Thread thread = new Thread(new ThreadStart(action));
            thread.Name = name;
            thread.Priority = priority;
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }

        public static Thread StartLoopedThread(string name, Action actionToLoop, Func<bool>? loopCondition = null, ThreadPriority priority = ThreadPriority.Normal)
        {
            if (loopCondition is null)
                loopCondition = () => Program.Instance.IsRunning;

            return StartThread(name, () =>
            {
                while (loopCondition())
                    actionToLoop();
            }, priority);
        }

        #endregion
    }
}
