using B.Utils;

namespace B.Options.Games.Adventure
{
    public sealed class Grid
    {
        public int RealWidth => this.Width * 2;
        public readonly int Width;
        public readonly int Height;

        private readonly Dict<Vector2, Action> _interactionDict = new();
        private readonly Dict<Vector2, (int, Vector2)> _doorDict = new();
        private readonly Tile[][] _tileGrid;
        private Vector2[] _coinList = new Vector2[0];

        // Private Initialization Cache
        private readonly int _initInteractables = 0;
        private readonly int _initDoors = 0;
        private bool _seald = false;

        public Grid(string[] raw)
        {
            if (raw.Length > 0)
            {
                this.Width = raw[0].Length;
                this.Height = raw.Length;
                this._tileGrid = new Tile[this.Height][];
                string str;
                char[] ca;
                Tile tile;
                Tile.TileTypes tileType;

                for (int y = 0; y < this.Height; y++)
                {
                    str = raw[y];

                    if (str.Length == this.Width)
                    {
                        this._tileGrid[y] = new Tile[this.Width];
                        ca = str.ToCharArray();

                        for (int x = 0; x < this.Width; x++)
                        {
                            tile = (Tile)ca[x];
                            this._tileGrid[y][x] = tile;
                            tileType = tile.TileType;

                            if (tileType == Tile.TileTypes.Coin)
                                _coinList = _coinList.Add(new Vector2(x, y));
                            else if (tileType == Tile.TileTypes.Interactable)
                                this._initInteractables++;
                            else if (tileType == Tile.TileTypes.Door)
                                this._initDoors++;
                        }
                    }
                    else
                        throw new ArgumentException("Grid Init Error: Rows must be same length");
                }
            }
            else
                throw new ArgumentException("Grid Init Error: Must have at least one row");
        }

        public Tile GetTile(Vector2 pos) => this._tileGrid[pos.y][pos.x];

        public bool HasCoinAt(Vector2 pos) => this._coinList.Contains(pos);

        public void PickupCoinAt(Vector2 pos)
        {
            _coinList = _coinList.Remove(pos);
            OptionAdventure.Info.Coins++;
            OptionAdventure.Message = "You picked up a coin!";
        }

        public void AddInteraction(Vector2 pos, Action action) => this.AddFeature(pos, action, "Interaction", tile => tile.TileType == Tile.TileTypes.Interactable, this._interactionDict);

        public void AddDoor(Vector2 pos, (int, Vector2) gridIdAndPos) => this.AddFeature(pos, gridIdAndPos, "Door", tile => tile.TileType == Tile.TileTypes.Door, this._doorDict);

        public void MoveTo(Vector2 pos)
        {
            if (this._seald)
            {
                Tile.TileTypes tileType = this.GetTile(pos).TileType;

                if (tileType == Tile.TileTypes.Coin && this._coinList.Contains(pos))
                    this.PickupCoinAt(pos);

                if (tileType == Tile.TileTypes.Door && this._doorDict.ContainsKey(pos))
                {
                    (int, Vector2) gridIdAndPos = this._doorDict[pos];
                    OptionAdventure.Info.GridID = gridIdAndPos.Item1;
                    OptionAdventure.Info.Position = gridIdAndPos.Item2 ?? Vector2.Zero;
                }
            }
            else
                throw new InvalidOperationException("Interact Error: Cannot move on unsealed grid");
        }

        public void Interact(Vector2 pos)
        {
            if (this._seald)
            {
                Tile.TileTypes tileType = this.GetTile(pos).TileType;

                if (tileType == Tile.TileTypes.Interactable && this._interactionDict.ContainsKey(pos))
                    this._interactionDict[pos]();
                else if (tileType == Tile.TileTypes.Coin && this._coinList.Contains(pos))
                    PickupCoinAt(pos);
            }
            else
                throw new InvalidOperationException("Interact Error: Cannot interact with unsealed grid");
        }

        public void Seal()
        {
            if (this._initDoors != this._doorDict.Length)
                throw new InvalidOperationException("Seal Error: Cannot seal grid with unimplemented doors");

            if (this._initInteractables != this._interactionDict.Length)
                throw new InvalidOperationException("Seal Error: Cannot seal grid with unimplemented interactables");

            this._seald = true;
        }

        private void AddFeature<T>(Vector2 pos, T obj, string name, Func<Tile, bool> check, Dict<Vector2, T> dict)
        {
            if (!this._seald)
            {
                if (check.Invoke(this.GetTile(pos)))
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

        public sealed override string ToString() => $"Grid: {this.Width}x{this.Height}";

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
