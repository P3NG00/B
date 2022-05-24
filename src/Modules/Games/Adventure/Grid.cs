using B.Utils;
using B.Utils.Extensions;

namespace B.Modules.Games.Adventure
{
    public sealed class Grid
    {
        #region Public Properties

        // Real console width of the grid.
        public int RealWidth => Width * 2;
        // Grid tile width.
        // Each tile is 2 chars wide.
        public readonly int Width;
        // Grid tile height.
        public readonly int Height;

        #endregion



        #region Private Variables

        // Dictionary of interactable tiles and their action.
        private readonly Dictionary<Vector2, Action> _interactionDict = new();
        // Dictionary of door tiles and their grid ID new position.
        private readonly Dictionary<Vector2, (int GridID, Vector2 NewPosition)> _doorDict = new();
        // List of coins on the grid.
        private readonly List<Vector2> _coinList = new();
        // 2D Tile Array of all grid tiles.
        private readonly Tile[][] _tileGrid;

        // Used for checking properly defined interactables.
        private readonly int _initInteractables = 0;
        // Used for checking properly defined doors.
        private readonly int _initDoors = 0;
        // Allows edits to be made to the grid.
        private bool _seald = false;

        #endregion



        #region Constructors

        // Creates a new Grid from string array of equal lengths.
        public Grid(string[] raw)
        {
            if (raw.Length == 0)
                throw new ArgumentException("Grid Init Error: Must have at least one row");

            Width = raw[0].Length;
            Height = raw.Length;
            _tileGrid = new Tile[Height][];
            string str;
            char[] ca;
            Tile tile;
            Tile.TileTypes tileType;

            for (int y = 0; y < Height; y++)
            {
                str = raw[y];

                if (str.Length != Width)
                    throw new ArgumentException("Grid Init Error: Rows must be same length");

                _tileGrid[y] = new Tile[Width];
                ca = str.ToCharArray();

                for (int x = 0; x < Width; x++)
                {
                    tile = (Tile)ca[x];
                    _tileGrid[y][x] = tile;
                    tileType = tile.TileType;

                    if (tileType == Tile.TileTypes.Coin)
                        _coinList.Add(new Vector2(x, y));
                    else if (tileType == Tile.TileTypes.Interactable)
                        _initInteractables++;
                    else if (tileType == Tile.TileTypes.Door)
                        _initDoors++;
                }
            }
        }

        #endregion



        #region Public Methods

        // Gets the tile at the given position.
        public Tile GetTile(Vector2 pos) => _tileGrid[pos.y][pos.x];

        // Returns if a coin exists at the given position.
        public bool HasCoinAt(Vector2 pos) => _coinList.Contains(pos);

        // Removes a coin from the given position.
        public void PickupCoinAt(Vector2 pos)
        {
            if (!_coinList.Remove(pos))
                throw new ArgumentException($"Coin does not exist at position '{pos}'!");

            ModuleAdventure.Info.Coins++;
            ModuleAdventure.Message = "You picked up a coin!";
        }

        // Adds an action to perform when the player interacts at the given position.
        public void AddInteraction(Vector2 pos, Action action) => AddFeature(pos, action, "Interaction", tile => tile.TileType == Tile.TileTypes.Interactable, _interactionDict);

        // Adds a door to change the player's grid ID and position at the given position.
        public void AddDoor(Vector2 pos, (int GridID, Vector2 NewPosition) gridIdAndPos) => AddFeature(pos, gridIdAndPos, "Door", tile => tile.TileType == Tile.TileTypes.Door, _doorDict);

        // Used to check interaction with the grid at a given position.
        public void CheckNewPlayerPosition(Vector2 pos)
        {
            if (!_seald)
                throw new InvalidOperationException("Interact Error: Cannot move on unsealed grid");

            Tile.TileTypes tileType = GetTile(pos).TileType;

            if (tileType == Tile.TileTypes.Coin && _coinList.Contains(pos))
                PickupCoinAt(pos);

            if (tileType == Tile.TileTypes.Door && _doorDict.ContainsKey(pos))
            {
                var gridIdAndPos = _doorDict[pos];
                ModuleAdventure.Info.GridID = gridIdAndPos.GridID;
                ModuleAdventure.Info.Position = gridIdAndPos.NewPosition ?? Vector2.Zero;
            }
        }

        // Used to check interaction with the grid at a given position.
        public void Interact(Vector2 pos)
        {
            if (!_seald)
                throw new InvalidOperationException("Interact Error: Cannot interact with unsealed grid");

            Tile.TileTypes tileType = GetTile(pos).TileType;

            if (tileType == Tile.TileTypes.Interactable && _interactionDict.ContainsKey(pos))
                _interactionDict[pos]();
            else if (tileType == Tile.TileTypes.Coin && _coinList.Contains(pos))
                PickupCoinAt(pos);
        }

        // Seals the grid for use in-game.
        // Changes cannot be made after sealing.
        public void Seal()
        {
            if (_initDoors != _doorDict.Count)
                throw new InvalidOperationException("Seal Error: Cannot seal grid with unimplemented doors");

            if (_initInteractables != _interactionDict.Count)
                throw new InvalidOperationException("Seal Error: Cannot seal grid with unimplemented interactables");

            _seald = true;
        }

        #endregion



        #region Private Methods

        // Adds details to a passed dictionary containing grid info.
        private void AddFeature<T>(Vector2 pos, T obj, string name, Func<Tile, bool> check, Dictionary<Vector2, T> dict)
        {
            if (_seald)
                throw new InvalidOperationException($"Add {name} Error: Cannot add {name} to a sealed grid");

            if (!check.Invoke(GetTile(pos)))
                throw new ArgumentException($"Add {name} Error: Tile is not {name} - {pos}");

            if (dict.ContainsKey(pos))
                throw new InvalidOperationException($"Add {name} Error: {name} already exists at {pos}");

            dict.Add(pos, obj);
        }

        #endregion



        #region Override Methods

        // Returns the string representation of the grid's size.
        public sealed override string ToString() => $"Grid: {Width}x{Height}";

        #endregion



        #region Universal Methods

        // Creates a whitespace string array as a template for grid creation.
        public static string[] CreateGrid(int width, int height)
        {
            string[] sa = new string[height];
            string s = ' '.Loop(width);
            for (int i = 0; i < sa.Length; i++)
                sa[i] = s;
            return sa;
        }

        #endregion
    }
}
