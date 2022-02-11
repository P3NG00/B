namespace B.Options.Settings
{
    public sealed class ColorTheme
    {
        public readonly string Title;
        public readonly ConsoleColor ColorText;
        public readonly ConsoleColor ColorBG;

        // TODO add different colors for Titles, Selected Scroll Index, etc.

        public ColorTheme(string title, ConsoleColor colorText, ConsoleColor colorBG)
        {
            this.Title = title;
            this.ColorText = colorText;
            this.ColorBG = colorBG;
        }
    }
}
