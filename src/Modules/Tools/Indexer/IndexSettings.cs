using B.Utils;
using Newtonsoft.Json;

namespace B.Modules.Tools.Indexer
{
    public sealed class IndexSettings
    {
        #region Public Properties

        [JsonProperty] public Togglable IndexOnStartup { get; private set; } = new();
        [JsonProperty] public Togglable IndexNetworkDrives { get; private set; } = new();
        // TODO add togglable to index compressed archives

        #endregion
    }
}
