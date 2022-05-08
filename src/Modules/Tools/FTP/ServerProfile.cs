using Newtonsoft.Json;

namespace B.Modules.Tools.FTP
{
    public sealed class ServerProfile
    {
        #region Public Properties

        [JsonIgnore] public string Path => ModuleFTP.DirectoryPath + Name;

        #endregion



        #region Public Variables

        [JsonIgnore] public string Name = null!;

        public string Username = null!;
        public string IP = null!;
        public int Port = 0;

        #endregion
    }
}
