using B.Utils;

namespace B.Options.MoneyTracker
{
    [Serializable]
    public sealed class Account
    {
        public string FilePath => OptionMoneyTracker.DirectoryPath + this.Name;
        public Utils.List<Transaction> Transactions = new Utils.List<Transaction>();
        public string Name = Util.NULL_STRING;
        public int Decimals
        {
            get => this._decimals;
            set => this._decimals = Util.Clamp(value, 0, Util.MAX_CHARS_DECIMAL);
        }

        private int _decimals = 2;

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

        public void PrintTransactions()
        {
            Util.Print();
            decimal total = 0m;

            foreach (Transaction transaction in this.Transactions)
            {
                total += transaction.Amount;
                Util.Print(string.Format("{0," + (Util.MAX_CHARS_DECIMAL + this.Decimals + 1) + ":0." + Util.StringOf("0", this.Decimals) + "} | {1," + Util.MAX_CHARS_DECIMAL + "}", transaction.Amount, transaction.Description), 2);
            }

            Util.Print("Total: " + total, 2, linesBefore: 1);
        }
    }
}
