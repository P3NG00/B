using B.Inputs;
using B.Utils;
using Newtonsoft.Json;

namespace B.Modules.Tools.MoneyTracker
{
    public sealed class Account
    {
        #region Public Properties

        [JsonIgnore] public string FilePath => ModuleMoneyTracker.DirectoryPath + Name;
        [JsonIgnore] public byte Decimals => _decimals;

        #endregion



        #region Public Variables

        public List<Transaction> Transactions = new();
        public string Name = string.Empty;

        #endregion



        #region Private Variables

        [JsonProperty] private byte _decimals = 2;

        #endregion



        #region Constructors

        // This is here for deserialization.
        // Accounts should be initialized with a name.
        private Account() { }

        public Account(string name) => Name = name;

        #endregion



        #region Public Methods

        public void Save() => Data.Serialize(FilePath, this);

        public void Delete()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        public void IncrementDecimals()
        {
            if (_decimals < Input.DECIMAL_LENGTH)
                _decimals++;
        }

        public void DecrementDecimals()
        {
            if (_decimals != 0)
                _decimals--;
        }

        #endregion
    }
}
