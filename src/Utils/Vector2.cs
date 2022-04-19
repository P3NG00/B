using System.Drawing;

namespace B.Utils
{
    [Serializable]
    public sealed class Vector2
    {
        public const int MaxStringLength = 11;

        public static Vector2 Up => new(0, 1);
        public static Vector2 Left => new(-1, 0);
        public static Vector2 Right => new(1, 0);
        public static Vector2 Down => new(0, -1);
        public static Vector2 Zero => new(0);
        public static Vector2 One => new(1);

        public int x;
        public int y;

        public Vector2 Copy() => new(this);

        public Vector2() : this(0) { }

        public Vector2(int square) : this(square, square) { }

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2(Vector2 vec) : this(vec.x, vec.y) { }

        public void Move(Vector2 vec)
        {
            x += vec.x;
            y += vec.y;
        }

        public override bool Equals(object? obj) => obj is Vector2 && this == (Vector2)obj;

        public override int GetHashCode() => (x * y).GetHashCode();

        public override string ToString() => $"({x},{y})";

        public static Vector2 Min(Vector2 vecA, Vector2 vecB) => new(Math.Min(vecA.x, vecB.x), Math.Min(vecA.y, vecB.y));

        public static Vector2 Max(Vector2 vecA, Vector2 vecB) => new(Math.Max(vecA.x, vecB.x), Math.Max(vecA.y, vecB.y));

        public static Vector2 operator +(Vector2 vecA, Vector2 vecB) => new(vecA.x + vecB.x, vecA.y + vecB.y);

        public static Vector2 operator -(Vector2 vecA, Vector2 vecB) => new(vecA.x - vecB.x, vecA.y - vecB.y);

        public static Vector2 operator *(Vector2 vecA, Vector2 vecB) => new(vecA.x * vecB.x, vecA.y * vecB.y);

        public static Vector2 operator *(Vector2 vec, int scale) => new(vec.x * scale, vec.y * scale);

        public static Vector2 operator *(Vector2 vec, float scale) => new((int)(vec.x * scale), (int)(vec.y * scale));

        public static Vector2 operator /(Vector2 vec, int scale) => new(vec.x / scale, vec.y / scale);

        public static Vector2 operator /(Vector2 vecA, Vector2 vecB) => new(vecA.x / vecB.x, vecA.y / vecB.y);

        public static bool operator ==(Vector2 vecA, Vector2 vecB) => vecA.x == vecB.x && vecA.y == vecB.y;

        public static bool operator !=(Vector2 vecA, Vector2 vecB) => !(vecA == vecB);

        public static implicit operator Vector2(Point point) => new(point.X, point.Y);

        public static implicit operator Vector2(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Vector2.Up;
                case Direction.Left: return Vector2.Left;
                case Direction.Down: return Vector2.Down;
                case Direction.Right: return Vector2.Right;
                default: return Vector2.Zero;
            }
        }
    }
}
