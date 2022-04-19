namespace B.Utils
{
    public static class Mouse
    {
        #region Universal Properties

        // This represents the size of one char in the console
        public static Vector2 CharSize => new(8, 16);
        // This will return the position of the mouse relative
        // to the top-left corner of the text window
        public static Vector2 Position => External.GetRelativeMousePosition() - TextBeginOffset;

        #endregion



        #region Private Properties

        // This represents the offset from the true top left corner of
        // the window to the top-left corner of where the text begins.
        private static Vector2 TextBeginOffset => new Vector2(1, 2) * CharSize;

        #endregion
    }
}
