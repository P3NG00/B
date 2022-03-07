using B.Utils;

namespace B.Options.Tools.MoneyTracker
{
    [Serializable]
    public sealed class Account
    {
        public string FilePath => OptionMoneyTracker.DirectoryPath + this.Name;
        public Utils.List<Transaction> Transactions = new();
        public string Name = string.Empty;
        public byte Decimals
        {
            get => this._decimals;
            set => this._decimals = value.Clamp(0, Constants.MAX_CHARS_DECIMAL);
        }

        private byte _decimals = 2;

        // This is here for deserialization.
        // Accounts should be initialized with a name.
        private Account() { }

        public Account(string name) => this.Name = name;

        public void Save() => Util.Serialize(this.FilePath, this);

        public void Delete()
        {
            if (File.Exists(this.FilePath))
                File.Delete(this.FilePath);
        }
    }
}
