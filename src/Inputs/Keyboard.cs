using B.Utils;

namespace B.Inputs
{
    public static class Keyboard
    {
        #region Universal Variables

        public static ConsoleKeyInfo LastInput { get; private set; } = default(ConsoleKeyInfo);

        #endregion



        #region Universal Methods

        public static void Initialize()
        {
            // Start thread to constantly accept keyboard input
            Util.StartLoopedThread(KeyboardThreadLoop);
        }

        #endregion



        #region Private Methods

        private static void KeyboardThreadLoop()
        {
            LastInput = Console.ReadKey(true);

            if (LastInput.Key == ConsoleKey.F12)
            {
                Program.Settings.DebugMode.Toggle();
                return;
            }

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
