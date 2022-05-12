namespace B.Utils.Extensions
{
    public static class IEnumerableB
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => enumerable.Count() == 0;

        public static T Random<T>(this IEnumerable<T> enumerable) => enumerable.ElementAt(enumerable.RandomIndex());

        public static int RandomIndex<T>(this IEnumerable<T> enumerable) => Util.Random.Next(enumerable.Count());

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T t in enumerable)
                action(t);
        }

        public static IEnumerable<V> FromEach<T, V>(this IEnumerable<T> enumerable, Func<T, V> getValueFrom)
        {
            foreach (T t in enumerable)
                yield return getValueFrom(t);
        }

        public static T FromRemainder<T>(this IEnumerable<T> enumerable, int i) => enumerable.ElementAt(i % enumerable.Count());
    }
}
