using B.Utils;

namespace B.Options.Toys.Canvas
{
    [Serializable]
    public sealed class CanvasInfo
    {
        public string Title = string.Empty;
        public ConsoleColor[][] Colors = null!;

        public string FilePath => OptionCanvas.DirectoryPath + Title;
        public Vector2 Size => new(Colors[0].Length, Colors.Length);

        // Use these as Color Getter and Setter
        public ref ConsoleColor Color(int x, int y) => ref Colors[y][x];
        public ref ConsoleColor Color(Vector2 pos) => ref Color(pos.x, pos.y);

        public void Draw(Vector2? offset = null)
        {
            Vector2 windowSize = Size + (OptionCanvas.CANVAS_BORDER_PAD * 2);

            if (offset is not null)
                windowSize += offset;

            Window.ClearAndSetSize(windowSize);

            for (int y = 0; y < Size.y; y++)
            {
                for (int x = 0; x < Size.x; x++)
                {
                    Vector2 newPos = new Vector2(x, y) + OptionCanvas.CANVAS_BORDER_PAD;

                    if (offset is not null)
                        newPos += offset;

                    Cursor.SetPosition(newPos);
                    Window.Print(' ', colorBG: Color(x, y));
                }
            }

            Cursor.Reset();
        }
    }
}
