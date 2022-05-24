namespace B.Modules.Games.MexicanTrain
{
    public sealed class Domino
    {
        #region Public Properties

        // Domino Value 1.
        public int Value1 => GetPointValue(_val1);
        // Domino Value 2.
        public int Value2 => GetPointValue(_val2);
        // Total Domino Value.
        public int Total => Value1 + Value2;

        #endregion



        #region Private Variables

        // Domino Value 1.
        private readonly int _val1;
        // Domino Value 2.
        private readonly int _val2;

        #endregion



        #region Constructors

        // Creates a new Domino with given values.
        public Domino(int val1, int val2)
        {
            _val1 = val1;
            _val2 = val2;
        }

        #endregion



        #region Private Methods

        // Gets the point value from the given domino value.
        // Only changes dominoes of 0 value to 25 points.
        private int GetPointValue(int val) => val == 0 ? 25 : val;

        #endregion
    }
}
