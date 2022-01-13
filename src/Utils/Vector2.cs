namespace B.Utils
{
    [Serializable]
    public sealed class Vector2 : Pair<int, int>
    {
        public static readonly Vector2 Up = new Vector2(0, 1);
        public static readonly Vector2 Left = new Vector2(-1, 0);
        public static readonly Vector2 Right = new Vector2(1, 0);
        public static readonly Vector2 Down = new Vector2(0, -1);
        public static readonly Vector2 Zero = new Vector2(0);

        public int x { get => this.Item1; set => this.Item1 = value; }
        public int y { get => this.Item2; set => this.Item2 = value; }

        public Vector2() : this(0) { }

        public Vector2(int xy) : this(xy, xy) { }

        public Vector2(int x, int y) : base(x, y) { }

        public override int GetHashCode() => (this.x * this.y).GetHashCode();

        public override bool Equals(object? obj) => obj is Vector2 && this == (Vector2)obj;

        public static Vector2 operator +(Vector2 vecA, Vector2 vecB) => new Vector2(vecA.x + vecB.x, vecA.y + vecB.y);

        public static bool operator ==(Vector2 vecA, Vector2 vecB) => vecA.x == vecB.x && vecA.y == vecB.y;

        // This isn't utilized, but needs to exist for '==' to work
        public static bool operator !=(Vector2 vecA, Vector2 vecB) => !(vecA == vecB);

        public sealed override string ToString() => string.Format("({0}, {1})", this.x, this.y);

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
