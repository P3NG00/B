namespace B.Modules.Games.Adventure
{
    public sealed class Tile
    {
        #region Private Variables

        private static readonly Dictionary<char, Tile> TileMap = new();

        #endregion



        #region Public Properties

        public readonly string Chars;
        public readonly TileTypes TileType;

        #endregion



        #region Constructors

        static Tile()
        {
            TileMap.Add(' ', new(ModuleAdventure.CHAR_EMPTY, TileTypes.Empty));
            TileMap.Add('c', new(ModuleAdventure.CHAR_EMPTY, TileTypes.Coin));
            TileMap.Add('d', new(ModuleAdventure.CHAR_DOOR, TileTypes.Door));
            TileMap.Add('w', new(ModuleAdventure.CHAR_WALL, TileTypes.Wall));
            TileMap.Add('i', new(ModuleAdventure.CHAR_INTERACTABLE, TileTypes.Interactable));
        }

        public Tile(string chars, TileTypes tileType)
        {
            if (chars.Length != 2)
                throw new ArgumentException("chars.Length != 2");

            Chars = chars;
            TileType = tileType;
        }

        #endregion



        #region Override Methods

        public sealed override string ToString() => $"Tile: chars:'{Chars}', stopMovement: {TileType.StopsMovement()}, isInteractable: {TileType.IsInteractable()}";

        #endregion



        #region Operator Overrides

        public static explicit operator Tile(char c)
        {
            try { return TileMap[c]; }
            catch (ArgumentException) { throw new ArgumentException($"Invalid tile character \"{c}\""); }
        }

        #endregion



        #region Enums

        public enum TileTypes
        {
            Empty,
            Wall,
            Coin,
            Door,
            Interactable
        }

        #endregion
    }

    public static class TileTypeFunc
    {
        public static bool StopsMovement(this Tile.TileTypes tileType) { return tileType == Tile.TileTypes.Wall || tileType == Tile.TileTypes.Interactable; }
        public static bool IsInteractable(this Tile.TileTypes tileType) { return tileType == Tile.TileTypes.Interactable || tileType == Tile.TileTypes.Coin; }
    }
}
