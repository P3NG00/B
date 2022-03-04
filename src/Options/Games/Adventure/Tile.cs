using B.Utils;

namespace B.Options.Games.Adventure
{
    [Serializable]
    public sealed class Tile
    {
        private static readonly Dict<char, Tile> TileMap = new();

        static Tile()
        {
            Tile.TileMap.Add(' ', new(OptionAdventure.CHAR_EMPTY, TileTypes.Empty));
            Tile.TileMap.Add('c', new(OptionAdventure.CHAR_EMPTY, TileTypes.Coin));
            Tile.TileMap.Add('d', new(OptionAdventure.CHAR_DOOR, TileTypes.Door));
            Tile.TileMap.Add('w', new(OptionAdventure.CHAR_WALL, TileTypes.Wall));
            Tile.TileMap.Add('i', new(OptionAdventure.CHAR_INTERACTABLE, TileTypes.Interactable));
        }

        public readonly string Chars;
        public readonly TileTypes TileType;

        public Tile(string chars, TileTypes tileType)
        {
            if (chars.Length != 2)
                throw new ArgumentException("chars.Length != 2");

            this.Chars = chars;
            this.TileType = tileType;
        }

        public enum TileTypes
        {
            Empty,
            Wall,
            Coin,
            Door,
            Interactable
        }

        public sealed override string ToString() => $"Tile: chars:'{this.Chars}', stopMovement: {this.TileType.StopsMovement()}, isInteractable: {this.TileType.IsInteractable()}";

        public static explicit operator Tile(char c)
        {
            try { return Tile.TileMap[c]; }
            catch (ArgumentException) { throw new ArgumentException($"Invalid tile character \"{c}\""); }
        }
    }

    public static class TileTypeFunc
    {
        public static bool StopsMovement(this Tile.TileTypes tileType) { return tileType == Tile.TileTypes.Wall || tileType == Tile.TileTypes.Interactable; }
        public static bool IsInteractable(this Tile.TileTypes tileType) { return tileType == Tile.TileTypes.Interactable || tileType == Tile.TileTypes.Coin; }
    }
}
