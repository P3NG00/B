using B.Utils;

namespace B.Options.Adventure
{
    [Serializable]
    public sealed class AdventureInfo
    {
        // TODO keep track of Tuple<gridId (int), coinPos (vector2)> to fix coin respawn on reload game

        public int GridID
        {
            get => this._gridID;
            set
            {
                this._gridID = value;
                // The console should be cleared if the GridID is changed because
                // the grid needs to be replaced completely instead of being
                // written on top of in case there is left over artifacts.
                Window.Clear();
            }
        }

        public int Speed
        {
            get => this._speed;
            set => this._speed = Util.Clamp(value, 1, 25);
        }

        public Vector2 Position = new();
        public int Coins = 0;

        private int _speed = 1;
        private int _gridID = 0;
    }
}
