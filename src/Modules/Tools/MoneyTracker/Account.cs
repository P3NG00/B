using B.Inputs;
using B.Utils;
using B.Utils.Extensions;
using Newtonsoft.Json;

namespace B.Modules.Tools.MoneyTracker
{
    public sealed class Account
    {
        #region Public Variables

        [JsonIgnore] public string FilePath => ModuleMoneyTracker.DirectoryPath + Name;
        public List<Transaction> Transactions = new();
        public string Name = string.Empty;
        public byte Decimals
        {
            get => _decimals;
            set => _decimals = value.Clamp(0, Input.DECIMAL_LENGTH);
        }

        #endregion



        #region Private Variables

        private byte _decimals = 2;

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

        #endregion
    }
}
