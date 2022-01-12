using B.Utils;

namespace B.Options.Adventure
{
    [Serializable]
    public class AdventureInfo
    {
        // TODO keep track of Tuple<gridId (int), coinPos (vector2)>

        public int GridID
        {
            get { return this._gridID; }
            set
            {
                this._gridID = value;
                Console.Clear();
            }
        }

        public int Speed
        {
            get { return this._speed; }
            set { this._speed = Util.Clamp(value, 1, 25); }
        }

        public Vector2 Position = new Vector2();
        public int Coins = 0;

        private int _speed = 1;
        private int _gridID = 0;
    }
}
