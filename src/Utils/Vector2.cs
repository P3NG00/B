namespace B.Utils
{
    [Serializable]
    public class Vector2
    {
        public static readonly Vector2 Up = new Vector2(0, 1);
        public static readonly Vector2 Left = new Vector2(-1, 0);
        public static readonly Vector2 Right = new Vector2(1, 0);
        public static readonly Vector2 Down = new Vector2(0, -1);
        public static readonly Vector2 Zero = new Vector2(0);

        public int x, y;

        public Vector2() : this(0) { }

        public Vector2(int xy) : this(xy, xy) { }

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override int GetHashCode() { return (this.x * this.y).GetHashCode(); }

        public override bool Equals(object obj) { return obj is Vector2 && this == (Vector2)obj; }

        public static Vector2 operator +(Vector2 vecA, Vector2 vecB) { return new Vector2(vecA.x + vecB.x, vecA.y + vecB.y); }

        public static bool operator ==(Vector2 vecA, Vector2 vecB) { return vecA.x == vecB.x && vecA.y == vecB.y; }

        // This isn't utilized, but needs to exist for '==' to work
        public static bool operator !=(Vector2 vecA, Vector2 vecB) { return !(vecA == vecB); }

        public sealed override string ToString() { return string.Format("({0}, {1})", this.x, this.y); }

        public static explicit operator Vector2(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Vector2.Up;
                case Direction.Left: return Vector2.Left;
                case Direction.Down: return Vector2.Down;
                case Direction.Right: return Vector2.Right;
            }

            return Vector2.Zero;
        }
    }
}
