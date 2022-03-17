using TextCopy;

namespace B.Utils
{
    public static class Clipboard
    {
        public static string Text
        {
            get
            {
                string? text = ClipboardService.GetText();
                return text is null ? string.Empty : text;
            }
            set => ClipboardService.SetText(value);
        }
    }
}
