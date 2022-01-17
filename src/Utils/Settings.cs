namespace B.Utils
{
    public sealed class Settings
    {
        public static string Path => Program.PathData + "settings";

        public bool DebugMode = false;
        public bool DarkMode = false;
    }
}
