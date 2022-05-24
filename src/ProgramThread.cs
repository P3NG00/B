namespace B
{
    // Interacts with multi-threading and creating new threads.
    public static class ProgramThread
    {
        #region Private Variables

        // Object used for thread-locking.
        private static object _lock = new();

        #endregion



        #region Private Properties

        // If the current thread is locked.
        private static bool Locked => Monitor.IsEntered(_lock);

        #endregion



        #region Universal Methods

        // Locks threads to run specified action.
        public static void Lock(Action action)
        {
            lock (_lock)
                action();
        }

        // If thread isn't locked, lock.
        public static void TryLock()
        {
            if (!Locked)
                Monitor.Enter(_lock);
        }

        // If thread is locked, unlock.
        public static void TryUnlock()
        {
            if (Locked)
                Monitor.Exit(_lock);
        }

        // Make thread wait specified amount of seconds.
        public static void Wait(float seconds = 0.01f)
        {
            int milliseconds = (int)(seconds * 1000f);
            Thread.Sleep(milliseconds);
        }

        // Create a new thread.
        public static Thread StartThread(string name, ThreadStart action, ThreadPriority priority = ThreadPriority.Normal)
        {
            Thread thread = new(action);
            thread.Name = name;
            thread.Priority = priority;
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }

        // Create a new thread that loops until specified condition is met.
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
