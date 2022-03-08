namespace B.Options.Games.MexicanTrain
{
    public sealed class Domino
    {
        public int Value1 => GetValue(_val1);
        public int Value2 => GetValue(_val2);
        public int Total => Value1 + Value2;

        private readonly byte _val1;
        private readonly byte _val2;

        public Domino(byte val1, byte val2)
        {
            _val1 = val1;
            _val2 = val2;
        }

        private int GetValue(byte val) => val == 0 ? 25 : val;
    }
}
