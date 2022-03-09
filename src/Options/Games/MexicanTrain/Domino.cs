namespace B.Options.Games.MexicanTrain
{
    public sealed class Domino
    {
        public int Value1 => GetValue(_val1);
        public int Value2 => GetValue(_val2);
        public int Total => Value1 + Value2;

        private readonly int _val1;
        private readonly int _val2;

        public Domino(int val1, int val2)
        {
            _val1 = val1;
            _val2 = val2;
        }

        private int GetValue(int val) => val == 0 ? 25 : val;
    }
}
