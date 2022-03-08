namespace B.Utils
{
    public static class Mathb
    {
        public static int Clamp(this int value, int min, int max) => Math.Clamp(value, min, max);

        public static byte Clamp(this byte value, byte min, byte max) => Math.Clamp(value, min, max);

        public static Vector2 Clamp(this Vector2 vec, Vector2 min, Vector2 max) => new(vec.x.Clamp(min.x, max.x), vec.y.Clamp(min.y, max.y));
    }
}
