using B.Utils;
using B.Utils.Extensions;

namespace B.Options.Games.Adventure
{
    public sealed class AdventureInfo
    {
        #region TODOs

        // TODO each grid will have a certain number of coins in it.
        // TODO the coins in each grid will be loaded/saved using an int as a bitfield.
        // TODO each grid will only have as many bits as the number of coins in it.

        #endregion



        #region Public Properties

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

        #endregion



        #region Public Variables

        public Vector2 Position = new();
        public int Coins = 0;

        #endregion



        #region Private Variables

        private int _speed = 1;
        private int _gridID = 0;

        #endregion
    }
}
