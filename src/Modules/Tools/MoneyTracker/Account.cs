using B.Inputs;
using B.Utils;
using Newtonsoft.Json;

namespace B.Modules.Tools.MoneyTracker
{
    public sealed class Account
    {
        #region Public Properties

        // Relative file path of this account.
        [JsonIgnore] public string FilePath => ModuleMoneyTracker.DirectoryPath + Name;
        // Amount of decimals to display in transaction view.
        [JsonIgnore] public byte Decimals => _decimals;

        #endregion



        #region Public Variables

        // Stored transactions.
        public List<Transaction> Transactions = new();
        // Account Name.
        public string Name = string.Empty;

        #endregion



        #region Private Variables

        // Stores amount of decimals to display in transaction view.
        [JsonProperty] private byte _decimals = 2;

        #endregion



        #region Constructors

        // This is here for deserialization.
        // Accounts should be initialized with a name.
        private Account() { }

        // Creates a new Account instance.
        public Account(string name) => Name = name;

        #endregion



        #region Public Methods

        // Saves the account file.
        public void Save() => Data.Serialize(FilePath, this);

        // Deletes the account file.
        public void Delete()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        // Increments the amount of decimals to display.
        public void IncrementDecimals()
        {
            if (_decimals < Input.DECIMAL_LENGTH)
                _decimals++;
        }

        // Decrements the amount of decimals to display.
        public void DecrementDecimals()
        {
            if (_decimals != 0)
                _decimals--;
        }

        #endregion
    }
}
