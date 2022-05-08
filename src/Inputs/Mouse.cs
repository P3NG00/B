using B.Utils;
using B.Utils.Extensions;

namespace B.Inputs
{
    public static class Mouse
    {
        #region Universal Variables

        // Used to determine what type of mouse input should be passed
        public static volatile MouseInputType InputType = MouseInputType.Click;

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

        public static bool KeybindsNeedRedraw => _lastMousePos == Vector2.Zero;
        public static bool LeftButtonDown => _lastLeftButtonDown;

        #endregion



        #region Private Variables

        private static bool _lastLeftButtonDown = false;
        private static bool _lastLeftButtonClick = false;
        private static Thread _thread = null!;
        private static Vector2 _lastMousePos = Vector2.Zero;

        #endregion



        #region Universal Methods

        public static void Initialize()
        {
            // Init check
            if (_thread is not null)
                throw new Exception("Mouse Capture Thread already initialized!");

            // Start Mouse Tracking Thread
            _thread = ProgramThread.StartLoopedThread(nameof(MouseThreadLoop), MouseThreadLoop, priority: ThreadPriority.BelowNormal);
        }

        // This resets the variable used to check if the mouse has moved to check the keybind highlights
        public static void MarkKeybindsForRedraw() => _lastMousePos = Vector2.Zero;

        #endregion



        #region Private Methods

        private static void MouseThreadLoop()
        {
            // Wait thread
            ProgramThread.Wait(_lastLeftButtonClick ? 0.1f : 0.01f);
            // Lock and process
            ProgramThread.Lock(Process);
        }

        private static void Process()
        {
            // Check state of mouse button
            bool currentLeftButton = External.GetLeftMouseButtonDown();
            // Compare last mouse state with new mouse state to determine if a click occurred
            _lastLeftButtonClick = currentLeftButton && !_lastLeftButtonDown;
            // Update last mouse state
            _lastLeftButtonDown = currentLeftButton;
            // If clicked, attempt to activate keybind
            if (_lastLeftButtonDown)
            {
                // If valid mouse input, set action to void to process input
                if (InputType == MouseInputType.Hold && Input.Action is null)
                    Input.Action = Util.Void;
                // If left mouse button clicked, test for keybind to activate
                if (_lastLeftButtonClick)
                    Keybind.FindKeybind(keybind => keybind.IsHighlighted);
            }
            // Get mouse position
            Vector2 mousePos = Position;
            // Print keybinds if mouse moved and not clicked
            if (mousePos != _lastMousePos && !_lastLeftButtonClick)
                Keybind.PrintRegisteredKeybinds();
            // Update last mouse position
            _lastMousePos = mousePos;
            // Mouse Position Debug display
            if (Program.Settings.DebugMode)
            {
                string positionStr = Position.ToString();
                int blankSpace = Vector2.MAX_STRING_LENGTH - positionStr.Length;
                // Top-right of text window
                Cursor.y = 0;
                Cursor.x = Window.Width - Vector2.MAX_STRING_LENGTH;
                // Overwrite area of text
                Window.Print(' '.Loop(blankSpace));
                // Display mouse position
                Window.Print(positionStr);
            }
        }

        #endregion



        #region Enums

        public enum MouseInputType
        {
            Click,
            Hold,
        }

        #endregion
    }
}
