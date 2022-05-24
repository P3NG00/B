using B.Utils;
using B.Utils.Themes;
using Newtonsoft.Json;

namespace B.Modules.Toys.Canvas
{
    public sealed class CanvasInfo
    {
        #region Public Variables

        // Canvas Title.
        [JsonIgnore] public string Title = string.Empty;
        // 2D Array of ConsoleColors that represent the image of the canvas.
        public ConsoleColor[][] Colors = null!;

        #endregion



        #region Public Properties

        // Relative path of where this Canvas is stored.
        [JsonIgnore] public string FilePath => ModuleCanvas.DirectoryPath + Title;
        // Size of the Canvas.
        [JsonIgnore] public Vector2 Size => new(Width, Height);
        // Width of the Canvas.
        [JsonIgnore] public int Width => Colors[0].Length;
        // Height of the Canvas.
        [JsonIgnore] public int Height => Colors.Length;

        #endregion



        #region Public Methods

        // Color Getter and Setter
        public ref ConsoleColor Color(int x, int y) => ref Colors[y][x];
        public ref ConsoleColor Color(Vector2 pos) => ref Colors[pos.y][pos.x];

        // Draws each ConsoleColor of the Canvas.
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

        // Serializes the canvas to file
        public void Save() => Data.Serialize(FilePath, this);

        #endregion
    }
}
