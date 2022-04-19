using B.Utils;

namespace B.Inputs
{
    public static class Mouse
    {
        #region Private Variables

        private static List<SelectableBox> _boxes = new();
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

            // Create Mouse Tracking Thread
            _thread = new Thread(() =>
            {
                // Mouse thread only runs while Program is running.
                // Once Program is stopped, thread finishes while loop and returns.
                while (Program.Instance.IsRunning)
                {
                    // If text is being drawn, wait until finished.
                    if (Window.IsDrawing)
                        continue;

                    // Mouse Position Debug
                    if (Program.Settings.DebugMode.Active)
                    {
                        Cursor.y = 0;
                        Cursor.x = Window.Width - Vector2.MaxStringLength;
                        Window.Print($"{Position.ToString(),Vector2.MaxStringLength}");
                    }
                }
            });

            // Start Mouse Tracking Thread
            _thread.Start();
        }

        public static void AddSelectableBox(SelectableBox box)
        {
            // TODO test
            _boxes.Add(box);
        }

        public static void ClearSelectableBoxes()
        {
            // TODO test
            _boxes.Clear();
        }

        #endregion
    }
}
