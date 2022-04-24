using B.Utils;

namespace B.Inputs
{
    public static class Keyboard
    {
        #region Private Variables

        private static Thread _thread = null!;

        #endregion



        #region Universal Variables

        public static ConsoleKeyInfo LastInput { get; private set; } = default(ConsoleKeyInfo);

        #endregion



        #region Universal Methods

        public static void Initialize()
        {
            // Init check
            if (_thread is not null)
                throw new Exception("Mouse Capture Thread already initialized!");

            // Start thread to constantly accept keyboard input
            _thread = ProgramThread.StartLoopedThread(KeyboardThreadLoop);
        }

        #endregion



        #region Private Methods

        private static void KeyboardThreadLoop()
        {
            // Get key
            LastInput = Console.ReadKey(true);

            ProgramThread.Lock();
            Process();
            ProgramThread.Unlock();
        }

        private static void Process()
        {
            // Make input void by default
            Input.Action = Util.Void;

            // Check debug key
            if (LastInput.Key == ConsoleKey.F12)
            {
                Input.Action = Program.Settings.DebugMode.Toggle;
                // Return to finish processing further input
                return;
            }

            // Find keybind
            foreach (Keybind keybind in Keybind.Keybinds)
            {
                if (keybind == LastInput)
                {
                    Input.Action = keybind.Action;
                    break;
                }
            }
        }

        #endregion
    }
}
