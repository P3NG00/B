using B.Utils;
using B.Utils.Extensions;

namespace B.Inputs
{
    public static class Mouse
    {
        #region Private Variables

        private static bool _lastMouseDown = false;
        private static Thread _thread = null!;

        #endregion



        #region Universal Properties

        // This will return the position of the mouse relative
        // to the top-left corner of the text window
        public static Vector2 Position
        {
            get
            {
                // This represents the offset from the true top left corner of
                // the window to the top-left corner of where the text begins.
                Vector2 textBeginOffset = new Vector2(1, 2) * Window.CharSize;
                Vector2 mousePosRelativeToTextWindow = External.GetRelativeMousePosition() - textBeginOffset;
                Vector2 scaledRelativeMousePos = mousePosRelativeToTextWindow / Window.CharSize;
                return scaledRelativeMousePos;
            }
        }

        #endregion



        #region Universal Methods

        public static void Initialize()
        {
            // Init check
            if (_thread is not null)
                throw new Exception("Mouse Capture Thread already initialized!");

            // Start Mouse Tracking Thread
            _thread = ProgramThread.StartLoopedThread(nameof(MouseThreadLoop), MouseThreadLoop, ThreadPriority.BelowNormal);
        }

        #endregion



        #region Private Methods

        private static void MouseThreadLoop()
        {
            ProgramThread.Wait();
            // Lock and process
            ProgramThread.Lock(Process);
        }

        private static void Process()
        {
            // First check for mouse click
            bool mouseDown = External.GetLeftMouseButtonDown();
            bool leftClick = mouseDown && !_lastMouseDown;

            // If clicked, attempt to activate keybind
            if (leftClick)
                Keybind.FindKeybind(keybind => keybind.IsHighlighted && keybind.Display);

            // Update last mouse state
            _lastMouseDown = mouseDown;

            // Print keybinds
            // (skip if clicked)
            if (!leftClick)
                Keybind.PrintRegisteredKeybinds();

            // Mouse Position Debug
            if (Program.Settings.DebugMode)
            {
                string positionStr = Position.ToString();
                int blankSpace = Vector2.MaxStringLength - positionStr.Length;
                Cursor.y = 0;
                Cursor.x = Window.Width - Vector2.MaxStringLength;
                Window.Print(' '.Loop(blankSpace));
                Window.Print(positionStr);
            }
        }

        #endregion
    }
}
