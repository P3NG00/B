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
        public static Vector2 Position => External.GetRelativeMousePosition() - TextBeginOffset;

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
                        string mouseCoords = (Position / Window.CharSize).ToString();
                        int offset = Window.Width - Vector2.MaxStringLength;
                        Cursor.y = 0;
                        Cursor.x = offset;
                        Window.Print($"{mouseCoords,Vector2.MaxStringLength}");
                    }

                    // TODO test if this makes a difference
                    Thread.Sleep(50);
                }

                // Thread is finished, dispose of reference
                _thread = null!;
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
