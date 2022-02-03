namespace B.Utils
{
    [Serializable]
    public class Pair<L, R>
    {
        public L? ItemLeft;
        public R? ItemRight;

        public Pair(L itemLeft, R itemRight)
        {
            this.ItemLeft = itemLeft;
            this.ItemRight = itemRight;
        }
    }
}
