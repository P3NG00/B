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

        // Returns if necessary for keybinds to be redrawn.
        // Keybinds will not be drawn unless the mouse is moved.
        public static bool KeybindsNeedRedraw => _lastMousePos == Vector2.Zero;
        // Returns if the left mouse button is being held.
        public static bool LeftButtonDown => _lastLeftButtonDown;

        #endregion



        #region Private Variables

        // Stores the last state of the left mouse button.
        private static bool _lastLeftButtonDown = false;
        // Stores if the left mouse button just registered a click.
        private static bool _lastLeftButtonClick = false;
        // Mouse processing thread reference.
        private static Thread _thread = null!;
        // Stores the last position of the mouse.
        private static Vector2 _lastMousePos = Vector2.Zero;

        #endregion



        #region Universal Methods

        // Initializes the Mouse Thread Loop.
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

        // Thread to loop for handling Mouse Input.
        private static void MouseThreadLoop()
        {
            // Wait thread
            ProgramThread.Wait(_lastLeftButtonClick ? 0.1f : 0.01f);
            // Lock and process
            ProgramThread.Lock(Process);
        }

        // Handles Mouse Input this frame.
        private static void Process()
        {
            // Check state of mouse button
            bool currentLeftButton = External.GetLeftMouseButtonDown();
            // Compare last mouse state with new mouse state to determine if a click occurred
            _lastLeftButtonClick = currentLeftButton && !_lastLeftButtonDown;
            // Update last mouse state
            _lastLeftButtonDown = currentLeftButton;
            // If left mouse button is currently held
            if (_lastLeftButtonDown)
            {
                // If processing held mouse input, skip input processing to allow for keybind activation
                if (InputType == MouseInputType.Hold && Input.Action is null)
                    Input.SkipInput();
                // If left mouse button was clicked, test for keybind to activate
                if (_lastLeftButtonClick)
                    Keybind.ActivateKeybind(keybind => keybind.IsHighlighted);
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
                // Get cursor position
                Vector2 cursorPos = Cursor.Position;
                // Top-right of text window
                Cursor.y = 0;
                Cursor.x = Window.Width - Vector2.MAX_STRING_LENGTH;
                // Overwrite area of text
                Window.Print(' '.Loop(blankSpace));
                // Display mouse position
                Window.Print(positionStr);
                // Restore cursor position
                Cursor.Position = cursorPos;
            }
        }

        #endregion



        #region Enums

        // How the thread should process mouse input.
        public enum MouseInputType
        {
            // Only updates on mouse click.
            Click,
            // Updates constantly when the mouse is held.
            Hold,
        }

        #endregion
    }
}
