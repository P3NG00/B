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
                // Position of the mouse cursor relative to the beginning of the text window (top-left corner)
                Vector2 mousePosRelativeToTextWindow = External.GetRelativeMousePosition() - TextBeginOffset;
                Vector2 scaledRelativeMousePos = mousePosRelativeToTextWindow / Window.CharSize;
                return scaledRelativeMousePos;
            }
        }

        #endregion



        #region Private Properties

        // This represents the offset from the true top left corner of
        // the window to the top-left corner of where the text begins.
        private static Vector2 TextBeginOffset => new Vector2(1, 2) * Window.CharSize;

        #endregion



        #region Universal Methods

        public static void Initialize()
        {
            // Init check
            if (_thread is not null)
                throw new Exception("Mouse Capture Thread already initialized!");

            // Start Mouse Tracking Thread
            _thread = ProgramThread.StartLoopedThread("MouseThreadLoop", MouseThreadLoop, ThreadPriority.BelowNormal);
        }

        #endregion



        #region Private Methods

        private static void MouseThreadLoop()
        {
            ProgramThread.Wait();

            ProgramThread.Lock();
            Process();
            ProgramThread.Unlock();
        }

        private static void Process()
        {
            // First check for mouse click
            bool mouseDown = External.GetLeftMouseButtonDown();
            bool leftClick = mouseDown && !_lastMouseDown;

            // If clicked, attempt to activate highlighted box
            if (leftClick)
            {
                foreach (Keybind keybind in Keybind.Keybinds)
                {
                    if (keybind.IsHighlighted)
                    {
                        Input.Action = keybind.Action;
                        break;
                    }
                }
            }

            // Update last mouse state
            _lastMouseDown = mouseDown;

            // Print Selectable Boxes
            // (skip if clicked)
            if (!leftClick)
                Keybind.Keybinds.ForEach(keybind => keybind.Print());

            // Mouse Position Debug
            if (Program.Settings.DebugMode.Active)
            {
                string positionStr = Position.ToString();
                int blankSpace = Vector2.MaxStringLength - positionStr.Length;
                Cursor.y = 0;
                Cursor.x = Window.Width - Vector2.MaxStringLength;
                Window.Print(' '.Loop(blankSpace));
                PrintType printType = leftClick ? PrintType.Highlight : PrintType.General;
                Window.Print(positionStr, printType);
                // Slow thread briefly to display click flash
                if (leftClick)
                    ProgramThread.Wait(100);
            }
        }

        #endregion
    }
}
