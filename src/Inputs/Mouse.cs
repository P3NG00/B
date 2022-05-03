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

        public static bool LeftButtonDown => _lastLeftButtonDown;

        #endregion



        #region Private Variables

        private static bool _lastLeftButtonDown = false;
        private static bool _lastLeftButtonClick = false;
        private static Thread _thread = null!;

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
            // If mouse was clicked, wait to help prevent 
            if (_lastLeftButtonClick)
                ProgramThread.Wait(0.1f);

            // Lock and process
            ProgramThread.Lock(Process);
        }

        private static void Process()
        {
            // First check for mouse click
            bool currentLeftButton = External.GetLeftMouseButtonDown();
            // Compare last mouse state with new mouse state
            _lastLeftButtonClick = currentLeftButton && !_lastLeftButtonDown;
            // Update last mouse state
            _lastLeftButtonDown = currentLeftButton;

            // If clicked, attempt to activate keybind
            if (_lastLeftButtonClick)
            {
                // Set to void to ensure a click is passed
                Input.Action = Util.Void;
                // Attempt to find highlighted keybind
                Keybind.FindKeybind(keybind => keybind.IsHighlighted);
            }
            else if (InputType == MouseInputType.Hold && _lastLeftButtonDown && Input.Action is null)
                Input.Action = Util.Void;

            // Print keybinds
            // (skip if clicked)
            if (!_lastLeftButtonClick)
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



        #region Enums

        public enum MouseInputType
        {
            Click,
            Hold,
        }

        #endregion
    }
}
