namespace B.Utils
{
    public static class Mouse
    {
        private static Vector2 CharSize = new(8, 16);
        private static Vector2 WindowTrueStart => new Vector2(1, 2) * CharSize;

        // This will return the position of the mouse relative
        // to the top-left corner of the text window
        public static Vector2 Position
        {
            get
            {
                Vector2 mousePos = External.GetRelativeMousePosition();
                return mousePos - WindowTrueStart;
            }
        }
    }
}
