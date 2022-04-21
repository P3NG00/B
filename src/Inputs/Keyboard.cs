using B.Utils;

namespace B.Inputs
{
    public static class Keyboard
    {
        #region Private Variables

        private static volatile bool _isProcessing = false;
        private static Thread _thread = null!;

        #endregion



        #region Universal Variables

        public static ConsoleKeyInfo LastInput { get; private set; } = default(ConsoleKeyInfo);
        public static bool IsProcessing => _isProcessing;

        #endregion



        #region Universal Methods

        public static void Initialize()
        {
            // Init check
            if (_thread is not null)
                throw new Exception("Mouse Capture Thread already initialized!");

            // Start thread to constantly accept keyboard input
            Util.StartLoopedThread(KeyboardThreadLoop, out _thread);
        }

        #endregion



        #region Private Methods

        private static void KeyboardThreadLoop()
        {
            // Get key
            LastInput = Console.ReadKey(true);

            // Mark as processing
            // (Mark this so that Mouse will stop processing to allow keyboard input)
            _isProcessing = true;

            // Process thread
            Process();

            // Finish processing
            _isProcessing = false;
        }

        private static void Process()
        {
            // Wait for drawing and mouse to finish
            Util.WaitFor(() => !Window.IsDrawing && !Mouse.IsProcessing);

            // Check debug key
            if (LastInput.Key == ConsoleKey.F12)
            {
                Program.Settings.DebugMode.Toggle();
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
