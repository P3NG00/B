namespace B.Options.Tools.Settings
{
    public sealed class ColorTheme
    {
        public readonly string Title;
        public readonly ConsoleColor ColorText;
        public readonly ConsoleColor ColorBG;

        public ColorTheme(string title, ConsoleColor colorText, ConsoleColor colorBG)
        {
            this.Title = title;
            this.ColorText = colorText;
            this.ColorBG = colorBG;
        }
    }
}
