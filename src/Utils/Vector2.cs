namespace B.Utils
{
    [Serializable]
    public sealed class Vector2
    {
        public static Vector2 Up => new(0, 1);
        public static Vector2 Left => new(-1, 0);
        public static Vector2 Right => new(1, 0);
        public static Vector2 Down => new(0, -1);
        public static Vector2 Zero => new(0);
        public static Vector2 One => new(1);

        public int x;
        public int y;

        public Vector2 Copy() => new(this);
        public (int, int) Tuple => (x, y);

        public Vector2() : this(0) { }

        public Vector2(int square) : this(square, square) { }

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2((int, int) tuple) : this(tuple.Item1, tuple.Item2) { }

        public Vector2(Vector2 vec) : this(vec.x, vec.y) { }

        public sealed override string ToString() => $"({x}, {y})";

        public void Move(Direction direction) => Move((Vector2)direction);

        public void Move(Vector2 vec) => Move(vec.x, vec.y);

        public void Move((int, int) tuple) => Move(tuple.Item1, tuple.Item2);

        public void Move(int x, int y)
        {
            x += x;
            y += y;
        }

        public override bool Equals(object? obj) => obj is Vector2 && this == (Vector2)obj;

        public override int GetHashCode() => (x * y).GetHashCode();

        public static Vector2 Min(Vector2 vecA, Vector2 vecB) => new(Math.Min(vecA.x, vecB.x), Math.Min(vecA.y, vecB.y));

        public static Vector2 Max(Vector2 vecA, Vector2 vecB) => new(Math.Max(vecA.x, vecB.x), Math.Max(vecA.y, vecB.y));

        public static Vector2 operator +(Vector2 vecA, Vector2 vecB) => new Vector2(vecA.x + vecB.x, vecA.y + vecB.y);

        public static Vector2 operator +(Vector2 vec, (int, int) tuple) => new Vector2(vec.x + tuple.Item1, vec.y + tuple.Item2);

        public static Vector2 operator -(Vector2 vecA, Vector2 vecB) => new Vector2(vecA.x - vecB.x, vecA.y - vecB.y);

        public static Vector2 operator -(Vector2 vec, (int, int) tuple) => new Vector2(vec.x - tuple.Item1, vec.y - tuple.Item2);

        public static Vector2 operator *(Vector2 vecA, Vector2 vecB) => new Vector2(vecA.x * vecB.x, vecA.y * vecB.y);

        public static Vector2 operator *(Vector2 vec, (int, int) tuple) => new Vector2(vec.x * tuple.Item1, vec.y * tuple.Item2);

        public static Vector2 operator *(Vector2 vec, int scale) => new(vec.x * scale, vec.y * scale);

        public static Vector2 operator /(Vector2 vec, int scale) => new(vec.x / scale, vec.y / scale);

        public static bool operator ==(Vector2 vecA, Vector2 vecB) => vecA.x == vecB.x && vecA.y == vecB.y;

        public static bool operator ==(Vector2 vec, (int, int) tuple) => vec.x == tuple.Item1 && vec.y == tuple.Item2;

        public static bool operator !=(Vector2 vecA, Vector2 vecB) => !(vecA == vecB);

        public static bool operator !=(Vector2 vec, (int, int) tuple) => !(vec == tuple);

        public static explicit operator Vector2((int x, int y) tuple) => new Vector2(tuple);

        public static explicit operator Vector2(Direction direction)
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
