using B.Utils;
using Newtonsoft.Json;

namespace B.Modules.Tools.Indexer
{
    public sealed class IndexSettings
    {
        #region Public Properties

        // Defines if the Indexer should run upon Program startup.
        [JsonProperty] public Togglable IndexOnStartup { get; private set; } = new(false);
        // Defines if the Indexer should index network drives.
        [JsonProperty] public Togglable IndexNetworkDrives { get; private set; } = new(false);

        #endregion
    }
}
