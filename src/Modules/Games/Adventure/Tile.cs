namespace B.Modules.Games.Adventure
{
    public sealed class Tile
    {
        #region Private Variables

        // Tile Map Dictionary.
        private static readonly Dictionary<char, Tile> TileMap = new();

        #endregion



        #region Public Properties

        // Tile Chars.
        public readonly string Chars;
        // Tile Type.
        public readonly TileTypes TileType;

        #endregion



        #region Constructors

        // Initializes the Tile Map.
        static Tile()
        {
            TileMap.Add(' ', new(ModuleAdventure.CHAR_EMPTY, TileTypes.Empty));
            TileMap.Add('c', new(ModuleAdventure.CHAR_EMPTY, TileTypes.Coin));
            TileMap.Add('d', new(ModuleAdventure.CHAR_DOOR, TileTypes.Door));
            TileMap.Add('w', new(ModuleAdventure.CHAR_WALL, TileTypes.Wall));
            TileMap.Add('i', new(ModuleAdventure.CHAR_INTERACTABLE, TileTypes.Interactable));
        }

        // Creates a new instance of Tile.
        public Tile(string chars, TileTypes tileType)
        {
            if (chars.Length != 2)
                throw new ArgumentException("chars.Length != 2");

            Chars = chars;
            TileType = tileType;
        }

        #endregion



        #region Override Methods

        // Returns information about the Tile as a string.
        public sealed override string ToString() => $"Tile: chars:'{Chars}', stopMovement: {TileType.StopsMovement()}, isInteractable: {TileType.IsInteractable()}";

        #endregion



        #region Operator Overrides

        // Enables the ability to cast a char to a Tile.
        public static explicit operator Tile(char c)
        {
            try { return TileMap[c]; }
            catch (ArgumentException) { throw new ArgumentException($"Invalid tile character \"{c}\""); }
        }

        #endregion



        #region Enums

        // Tile Types.
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
        // Returns if the Tile Type stops movement.
        public static bool StopsMovement(this Tile.TileTypes tileType) { return tileType == Tile.TileTypes.Wall || tileType == Tile.TileTypes.Interactable; }
        // Returns if the Tile Type is interactable.
        public static bool IsInteractable(this Tile.TileTypes tileType) { return tileType == Tile.TileTypes.Interactable || tileType == Tile.TileTypes.Coin; }
    }
}
