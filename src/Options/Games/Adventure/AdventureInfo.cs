using B.Utils;
using B.Utils.Extensions;

namespace B.Options.Games.Adventure
{
    public sealed class AdventureInfo
    {
        // TODO keep track of Tuple<gridId (int), coinPos (vector2)> to fix coin respawn on reload game

        public int GridID
        {
            get => _gridID;
            set
            {
                _gridID = value;
                // The console should be cleared if the GridID is changed because
                // the grid needs to be replaced completely instead of being
                // written on top of in case there is left over artifacts.
                Window.Clear();
            }
        }

        public int Speed
        {
            get => _speed;
            set => _speed = value.Clamp(1, 25);
        }

        public Vector2 Position = new();
        public int Coins = 0;

        private int _speed = 1;
        private int _gridID = 0;
    }
}
