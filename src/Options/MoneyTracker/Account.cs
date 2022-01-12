using B.Utils;

namespace B.Options.MoneyTracker
{
    [Serializable]
    sealed class Account
    {
        public readonly string Name;
        public List<Transaction> Transactions { get { return this._transactions; } }
        public int Decimals
        {
            get { return this._decimals; }
            set { this._decimals = Util.Clamp(value, 0, 8); }
        }

        private readonly List<Transaction> _transactions = new List<Transaction>();
        private readonly string _filePath;
        private int _decimals = 2;

        public Account(string name)
        {
            this.Name = name;
            this._filePath = OptionMoneyTracker.DirectoryPath + name;
        }

        public void Save() { Util.Serialize(this._filePath, this); }

        public void Delete() { if (File.Exists(this._filePath)) File.Delete(this._filePath); }

        public void PrintTransactions()
        {
            Util.Print();

            foreach (Account.Transaction transaction in this.Transactions)
                Util.Print(string.Format("{0," + (6 + this.Decimals) + ":0." + Util.StringOf("0", this.Decimals) + "} | {1,16}", transaction.Amount, transaction.Description), 2);
        }

        [Serializable]
        public sealed class Transaction
        {
            public string Description = string.Empty;
            public double Amount = 0d;

            public Transaction() { }
        }
    }
}
