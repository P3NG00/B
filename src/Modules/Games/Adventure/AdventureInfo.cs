using B.Utils;
using B.Utils.Extensions;

namespace B.Modules.Games.Adventure
{
    public sealed class AdventureInfo
    {
        #region Public Properties

        // ID of the grid the player is in.
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

        // Player tile movement speed.
        public int Speed
        {
            get => _speed;
            set => _speed = value.Clamp(1, 25);
        }

        #endregion



        #region Public Variables

        // Player's current position in the grid.
        public Vector2 Position = new();
        // Amount of collected coins.
        public int Coins = 0;

        #endregion



        #region Private Variables

        // Player tile movement speed.
        private int _speed = 1;
        // ID of the grid the player is in.
        private int _gridID = 0;

        #endregion
    }
}
