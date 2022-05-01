namespace B.Options.Games.MexicanTrain
{
    public sealed class Domino
    {
        #region Public Properties

        public int Value1 => GetValue(_val1);
        public int Value2 => GetValue(_val2);
        public int Total => Value1 + Value2;

        #endregion



        #region Private Variables

        private readonly int _val1;
        private readonly int _val2;

        #endregion



        #region Constructors

        public Domino(int val1, int val2)
        {
            _val1 = val1;
            _val2 = val2;
        }

        #endregion



        #region Private Methods

        private int GetValue(int val) => val == 0 ? 25 : val;

        #endregion
    }
}
