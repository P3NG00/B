namespace B.Options.MoneyTracker
{
    [Serializable]
    public sealed class Transaction
    {
        public string Description = string.Empty;
        public decimal Amount = 0m;

        public Transaction() { }
    }
}
