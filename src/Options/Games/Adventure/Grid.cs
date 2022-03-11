using B.Utils;

namespace B.Options.Games.Adventure
{
    public sealed class Grid
    {
        public int RealWidth => Width * 2;
        public readonly int Width;
        public readonly int Height;

        private readonly Dict<Vector2, Action> _interactionDict = new();
        private readonly Dict<Vector2, (int, Vector2)> _doorDict = new();
        private readonly List<Vector2> _coinList = new();
        private readonly Tile[][] _tileGrid;

        // Private Initialization Cache
        private readonly int _initInteractables = 0;
        private readonly int _initDoors = 0;
        private bool _seald = false;

        public Grid(string[] raw)
        {
            if (raw.Length > 0)
            {
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

                    if (str.Length == Width)
                    {
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
                    else
                        throw new ArgumentException("Grid Init Error: Rows must be same length");
                }
            }
            else
                throw new ArgumentException("Grid Init Error: Must have at least one row");
        }

        public Tile GetTile(Vector2 pos) => _tileGrid[pos.y][pos.x];

        public bool HasCoinAt(Vector2 pos) => _coinList.Contains(pos);

        public void PickupCoinAt(Vector2 pos)
        {
            _coinList.Remove(pos);
            OptionAdventure.Info.Coins++;
            OptionAdventure.Message = "You picked up a coin!";
        }

        public void AddInteraction(Vector2 pos, Action action) => AddFeature(pos, action, "Interaction", tile => tile.TileType == Tile.TileTypes.Interactable, _interactionDict);

        public void AddDoor(Vector2 pos, (int, Vector2) gridIdAndPos) => AddFeature(pos, gridIdAndPos, "Door", tile => tile.TileType == Tile.TileTypes.Door, _doorDict);

        public void MoveTo(Vector2 pos)
        {
            if (_seald)
            {
                Tile.TileTypes tileType = GetTile(pos).TileType;

                if (tileType == Tile.TileTypes.Coin && _coinList.Contains(pos))
                    PickupCoinAt(pos);

                if (tileType == Tile.TileTypes.Door && _doorDict.ContainsKey(pos))
                {
                    (int, Vector2) gridIdAndPos = _doorDict[pos];
                    OptionAdventure.Info.GridID = gridIdAndPos.Item1;
                    OptionAdventure.Info.Position = gridIdAndPos.Item2 ?? Vector2.Zero;
                }
            }
            else
                throw new InvalidOperationException("Interact Error: Cannot move on unsealed grid");
        }

        public void Interact(Vector2 pos)
        {
            if (_seald)
            {
                Tile.TileTypes tileType = GetTile(pos).TileType;

                if (tileType == Tile.TileTypes.Interactable && _interactionDict.ContainsKey(pos))
                    _interactionDict[pos]();
                else if (tileType == Tile.TileTypes.Coin && _coinList.Contains(pos))
                    PickupCoinAt(pos);
            }
            else
                throw new InvalidOperationException("Interact Error: Cannot interact with unsealed grid");
        }

        public void Seal()
        {
            if (_initDoors != _doorDict.Length)
                throw new InvalidOperationException("Seal Error: Cannot seal grid with unimplemented doors");

            if (_initInteractables != _interactionDict.Length)
                throw new InvalidOperationException("Seal Error: Cannot seal grid with unimplemented interactables");

            _seald = true;
        }

        private void AddFeature<T>(Vector2 pos, T obj, string name, Func<Tile, bool> check, Dict<Vector2, T> dict)
        {
            if (!_seald)
            {
                if (check.Invoke(GetTile(pos)))
                {
                    if (!dict.ContainsKey(pos))
                        dict.Add(pos, obj);
                    else
                        throw new InvalidOperationException($"Add {name} Error: {name} already exists at {pos}");
                }
                else
                    throw new ArgumentException($"Add {name} Error: Tile is not {name} - {pos}");
            }
            else
                throw new InvalidOperationException($"Add {name} Error: Cannot add {name} to a sealed grid");
        }

        public sealed override string ToString() => $"Grid: {Width}x{Height}";

        public static string[] CreateGrid(Vector2 dimensions)
        {
            string[] sa = new string[dimensions.y];
            string s = Util.StringOf(' ', dimensions.x);

            for (int i = 0; i < sa.Length; i++)
                sa[i] = s;

            return sa;
        }
    }
}
