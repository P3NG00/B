namespace B.Utils.Extensions
{
    public static class IEnumerableB
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => enumerable.Count() == 0;

        public static T Random<T>(this IEnumerable<T> enumerable) => enumerable.ElementAt(enumerable.RandomIndex());

        public static int RandomIndex<T>(this IEnumerable<T> enumerable) => Util.Random.Next(enumerable.Count());

        // TODO refactor my ForEach calls. multi-line ForEach should be replace with regular foreach loops
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action action)
        {
            foreach (T t in enumerable)
                action();
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T t in enumerable)
                action(t);
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
        {
            int i = 0;

            foreach (T t in enumerable)
            {
                action(t, i);
                i++;
            }
        }

        public static IEnumerable<V> FromEach<T, V>(this IEnumerable<T> enumerable, Func<T, V> getValueFrom)
        {
            foreach (T t in enumerable)
                yield return getValueFrom(t);
        }

        public static T FromRemainder<T>(this IEnumerable<T> enumerable, int i) => enumerable.ElementAt(i % enumerable.Count());
    }
}
