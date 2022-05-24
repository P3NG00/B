namespace B.Utils.Extensions
{
    public static class IEnumerableB
    {
        // Checks if given enumerable is empty.
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => enumerable.Count() == 0;

        // Retrieves a random element from given enumerable.
        public static T Random<T>(this IEnumerable<T> enumerable) => enumerable.ElementAt(enumerable.RandomIndex());

        // Gets a random number that will have a corresponding entry in given enumerable.
        public static int RandomIndex<T>(this IEnumerable<T> enumerable) => Util.Random.Next(enumerable.Count());

        // Does an action for each element in given enumerable.
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T t in enumerable)
                action(t);
        }

        // Uses the remainder of the provided integer divided by length of the given enumerable to retrieve an element.
        public static T FromRemainder<T>(this IEnumerable<T> enumerable, int i) => enumerable.ElementAt(i % enumerable.Count());
    }
}
