using B.Utils;
using B.Utils.Themes;
using Newtonsoft.Json;

namespace B.Options.Toys.Canvas
{
    public sealed class CanvasInfo
    {
        [JsonIgnore] public string Title = string.Empty;
        public ConsoleColor[][] Colors = null!;

        [JsonIgnore] public string FilePath => OptionCanvas.DirectoryPath + Title;
        [JsonIgnore] public Vector2 Size => new(Colors[0].Length, Colors.Length);

        // Use these as Color Getter and Setter
        public ref ConsoleColor Color(int x, int y) => ref Colors[y][x];
        public ref ConsoleColor Color(Vector2 pos) => ref Colors[pos.y][pos.x];

        public void Draw(Vector2? offset = null)
        {
            Vector2 windowSize = Size + (OptionCanvas.CANVAS_BORDER_PAD * 2);

            if (offset is not null)
                windowSize += offset;

            Window.Clear();
            Window.SetSize(windowSize);

            for (int y = 0; y < Size.y; y++)
            {
                for (int x = 0; x < Size.x; x++)
                {
                    Vector2 newPos = new Vector2(x, y) + OptionCanvas.CANVAS_BORDER_PAD;

                    if (offset is not null)
                        newPos += offset;

                    Cursor.Position = newPos;
                    Window.Print(' ', new ColorPair(colorBack: Color(x, y)));
                }
            }

            Cursor.Reset();
        }
    }
}
