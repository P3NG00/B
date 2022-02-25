using B.Utils;

namespace B.Options.Canvas
{
    [Serializable]
    public sealed class CanvasInfo
    {
        public string Title = string.Empty;
        public ConsoleColor[][] Colors = null!;

        public string FilePath => OptionCanvas.DirectoryPath + this.Title;
        public Vector2 Size => new(this.Colors[0].Length, this.Colors.Length);

        // Use these as Color Getter and Setter
        public ref ConsoleColor Color(int x, int y) => ref this.Colors[y][x];
        public ref ConsoleColor Color(Vector2 pos) => ref this.Color(pos.x, pos.y);

        public void Draw(Vector2? offset = null)
        {
            Vector2 windowSize = this.Size + (OptionCanvas.CANVAS_BORDER_PAD * 2);

            if (offset is not null)
                windowSize += offset;

            Window.ClearAndSetSize(windowSize);

            for (int y = 0; y < this.Size.y; y++)
            {
                for (int x = 0; x < this.Size.x; x++)
                {
                    Vector2 newPos = new Vector2(x, y) + OptionCanvas.CANVAS_BORDER_PAD;

                    if (offset is not null)
                        newPos += offset;

                    Cursor.Position = newPos;
                    Window.Print(' ', colorBackground: this.Color(x, y));
                }
            }

            Cursor.Reset();
        }
    }
}
