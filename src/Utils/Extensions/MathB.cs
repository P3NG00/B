namespace B.Utils.Extensions
{
    public static class MathB
    {
        /// Clamps an integer between a minimum and maximum value.
        public static int Clamp(this int value, int min, int max) => Math.Clamp(value, min, max);

        /// Clamps a byte between a minimum and maximum value.
        public static byte Clamp(this byte value, byte min, byte max) => Math.Clamp(value, min, max);

        /// Clamps both Vector2 values between minimum and maximum values.
        public static Vector2 Clamp(this Vector2 vec, Vector2 min, Vector2 max) => new(vec.x.Clamp(min.x, max.x), vec.y.Clamp(min.y, max.y));
    }
}
