using Newtonsoft.Json;

namespace B.Modules.Tools.FTP
{
    public sealed class ServerProfile
    {
        #region Public Properties

        // Relative file path of the loaded profile.
        [JsonIgnore] public string Path => ModuleFTP.DirectoryPath + Name;

        #endregion



        #region Public Variables

        // Profile name.
        [JsonIgnore] public string Name = null!;

        // Profile server username.
        public string Username = null!;
        // Profile server IP.
        public string IP = null!;
        // Profile server port.
        public int Port = 0;

        #endregion
    }
}
