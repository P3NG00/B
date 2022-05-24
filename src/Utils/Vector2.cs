using B.Utils.Enums;

namespace B.Utils
{
    public sealed class Vector2
    {
        #region Constants

        // Maximum length that a Vector2 will be printed as a string.
        public const int MAX_STRING_LENGTH = 11;

        #endregion



        #region Universal Properties

        // A Vector2 that represents 'up' on a 2D graph.
        public static Vector2 Up => new(0, 1);
        // A Vector2 that represents 'left' on a 2D graph.
        public static Vector2 Left => new(-1, 0);
        // A Vector2 that represents 'right' on a 2D graph.
        public static Vector2 Right => new(1, 0);
        // A Vector2 that represents 'down' on a 2D graph.
        public static Vector2 Down => new(0, -1);
        // A Vector2 with both values set to 0.
        public static Vector2 Zero => new(0);
        // A Vector2 with both values set to 1.
        public static Vector2 One => new(1);

        #endregion



        #region Public Variables

        // X coordinate value.
        public int x;
        // Y coordinate value.
        public int y;

        #endregion



        #region Constructor

        // Creates a Vector2 with both values initialized to 0.
        public Vector2() : this(0) { }

        // Creates a Vector2 with both values initialized to specified amount.
        public Vector2(int square) : this(square, square) { }

        // Creates a Vector2 with specified x and y values.
        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // Creates a new Vector2 with the same values as specified Vector2.
        public Vector2(Vector2 vec) : this(vec.x, vec.y) { }

        #endregion



        #region Public Methods

        // Adds values from specified Vector2 to this.
        public void Move(Vector2 vec)
        {
            x += vec.x;
            y += vec.y;
        }

        #endregion



        #region Override Methods

        // Compares this Vector2 to an object.
        public override bool Equals(object? obj) => obj is Vector2 && this == (Vector2)obj;

        // Calculates the hashcode using the x and y values of this Vector2.
        public override int GetHashCode() => (x * y).GetHashCode();

        // String representation of this Vector2.
        public override string ToString() => $"({x},{y})";

        #endregion



        #region Universal Methods

        // Calculates the smallest values of either Vector2.
        public static Vector2 Min(Vector2 vecA, Vector2 vecB) => new(Math.Min(vecA.x, vecB.x), Math.Min(vecA.y, vecB.y));

        // Calculates the largest values of either Vector2.
        public static Vector2 Max(Vector2 vecA, Vector2 vecB) => new(Math.Max(vecA.x, vecB.x), Math.Max(vecA.y, vecB.y));

        #endregion



        #region Operator Overrides

        // Adds Vector2s together.
        public static Vector2 operator +(Vector2 vecA, Vector2 vecB) => new(vecA.x + vecB.x, vecA.y + vecB.y);

        // Subtracts Vector2s.
        public static Vector2 operator -(Vector2 vecA, Vector2 vecB) => new(vecA.x - vecB.x, vecA.y - vecB.y);

        // Multiplies Vector2s together.
        public static Vector2 operator *(Vector2 vecA, Vector2 vecB) => new(vecA.x * vecB.x, vecA.y * vecB.y);

        // Multiplies Vector2 with integer scalar.
        public static Vector2 operator *(Vector2 vec, int scale) => new(vec.x * scale, vec.y * scale);

        // Multiplies Vector2 with float scalar.
        public static Vector2 operator *(Vector2 vec, float scale) => new((int)(vec.x * scale), (int)(vec.y * scale));

        // Divides Vector2s.
        public static Vector2 operator /(Vector2 vecA, Vector2 vecB) => new(vecA.x / vecB.x, vecA.y / vecB.y);

        // Divides Vector2 with integer scalar.
        public static Vector2 operator /(Vector2 vec, int scale) => new(vec.x / scale, vec.y / scale);

        // Compares two Vector2s for equality.
        public static bool operator ==(Vector2 vecA, Vector2 vecB) => vecA.x == vecB.x && vecA.y == vecB.y;

        // Compares two Vector2s for inequality.
        public static bool operator !=(Vector2 vecA, Vector2 vecB) => !(vecA == vecB);

        // Implicitly converts a Direction enum to the appropriate Vector2.
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

        #endregion
    }
}
