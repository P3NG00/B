using B.Inputs;
using B.Utils;
using B.Utils.Extensions;

namespace B.Options.Tools.MoneyTracker
{
    [Serializable]
    public sealed class Account
    {
        public string FilePath => OptionMoneyTracker.DirectoryPath + Name;
        public List<Transaction> Transactions = new();
        public string Name = string.Empty;
        public byte Decimals
        {
            get => _decimals;
            set => _decimals = value.Clamp(0, Input.DECIMAL_LENGTH);
        }

        private byte _decimals = 2;

        // This is here for deserialization.
        // Accounts should be initialized with a name.
        private Account() { }

        public Account(string name) => Name = name;

        public void Save() => Util.Serialize(FilePath, this);

        public void Delete()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}
