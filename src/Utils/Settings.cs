namespace B.Utils
{
    public sealed class ProgramSettings
    {
        public static string Path => Program.DataPath + "settings";

        public bool DebugMode = false;
        public bool DarkMode = false;
    }
}
