using B.Utils;
using B.Utils.Themes;
using Newtonsoft.Json;

namespace B.Options.Toys.Canvas
{
    public sealed class CanvasInfo
    {
        #region Public Variables

        [JsonIgnore] public string Title = string.Empty;
        public ConsoleColor[][] Colors = null!;

        #endregion



        #region Public Properties

        [JsonIgnore] public string FilePath => OptionCanvas.DirectoryPath + Title;
        [JsonIgnore] public Vector2 Size => new(Width, Height);
        [JsonIgnore] public int Width => Colors[0].Length;
        [JsonIgnore] public int Height => Colors.Length;

        #endregion



        #region Public Methods

        // Use these as Color Getter and Setter
        public ref ConsoleColor Color(int x, int y) => ref Colors[y][x];
        public ref ConsoleColor Color(Vector2 pos) => ref Colors[pos.y][pos.x];

        public void Draw()
        {
            Vector2 topLeft = Cursor.Position;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // Find positions
                    Vector2 canvasPos = new(x, y);
                    Vector2 windowPos = canvasPos + topLeft;
                    // Print appropriate color at position
                    Cursor.Position = windowPos;
                    Window.Print(' ', new ColorPair(colorBack: Color(canvasPos)));
                }
            }
        }

        #endregion
    }
}
