using TextCopy;

namespace B.Utils
{
    public static class Clipboard
    {
        #region Universal Properties

        // Retrieve and set a string on the clipboard.
        public static string Text
        {
            get => ClipboardService.GetText() ?? string.Empty;
            set => ClipboardService.SetText(value);
        }

        #endregion
    }
}
