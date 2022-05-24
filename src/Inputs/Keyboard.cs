using B.Utils;

namespace B.Inputs
{
    public static class Keyboard
    {
        #region Private Variables

        // Keyboard processing thread reference.
        private static Thread _thread = null!;

        #endregion



        #region Universal Variables

        // Most recent key to be pressed on the keyboard.
        public static ConsoleKeyInfo LastInput { get; private set; } = default;

        #endregion



        #region Universal Methods

        // Initializes the Keyboard Thread Loop.
        public static void Initialize()
        {
            // Init check
            if (_thread is not null)
                throw new Exception("Mouse Capture Thread already initialized!");

            // Start thread to constantly accept keyboard input
            _thread = ProgramThread.StartLoopedThread(nameof(KeyboardThreadLoop), KeyboardThreadLoop, priority: ThreadPriority.AboveNormal);
        }

        // Resets the last pressed key to the default value.
        // This is done to prevent double-input of the same key under certain circumstances.
        public static void ResetInput() => LastInput = default;

        #endregion



        #region Private Methods

        // Thread to loop for handling Keyboard Input.
        private static void KeyboardThreadLoop()
        {
            // Get key (thread hangs until key is pressed to continue processing)
            LastInput = Console.ReadKey(true);
            // Lock and process
            ProgramThread.Lock(Process);
        }

        // Handles Keyboard Input this frame.
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
            Keybind.ActivateKeybind(keybind => keybind == LastInput);
        }

        #endregion
    }
}
