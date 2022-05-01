namespace B.Options.Tools.MoneyTracker
{
    public sealed class Transaction
    {
        #region Public Variables

        public string Description = string.Empty;
        public decimal Amount = 0m;

        #endregion



        #region Constructors

        // TODO test and put comment if true "this needs to be here for serialization/deserialization purposes"
        public Transaction() { }

        #endregion
    }
}
