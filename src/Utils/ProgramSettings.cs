namespace B.Utils
{
    public sealed class ProgramSettings
    {
        public static string SettingsPath => Program.DirectoryPath + "settings";

        public bool DebugMode = false;
        public bool DarkMode = false;
    }
}
