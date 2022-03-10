namespace B.Utils.Extensions
{
    public static class Arrayb
    {
        // TODO remove
        public static T[] Add<T>(this T[] array, params T[] items)
        {
            T[] newArray = new T[array.Length + items.Length];
            Array.Copy(array, newArray, array.Length);
            Array.Copy(items, 0, newArray, array.Length, items.Length);
            return newArray;
        }

        // TODO remove
        public static T[] Add<T>(this T[] array, T item, int index)
        {
            T[] newArray = new T[array.Length + 1];
            Array.Copy(array, newArray, index);
            newArray[index] = item;
            Array.Copy(array, index, newArray, index + 1, array.Length - index);
            return newArray;
        }

        // TODO remove
        public static T[] Remove<T>(this T[] array, T item)
        {
            // Remove element from array by skipping it while iterating
            for (int i = 0; i < array.Length; i++)
            {
                T currentItem = array[i];

                if (currentItem is not null && currentItem.Equals(item))
                    return array.Remove(i);
            }

            return array;
        }

        // TODO remove
        public static T[] Remove<T>(this T[] array, int index)
        {
            T[] newArray = new T[array.Length - 1];
            Array.Copy(array, newArray, index);
            Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
            return newArray;
        }

        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            foreach (T item in array)
                action(item);
        }

        public static void ForEach<T>(this T[] array, Action<T, int> action)
        {
            for (int i = 0; i < array.Length; i++)
                action(array[i], i);
        }

        public static V[] FromEach<T, V>(this T[] array, Func<T, V> action)
        {
            V[] subArray = new V[array.Length];

            for (int i = 0; i < array.Length; i++)
                subArray[i] = action(array[i]);

            return subArray;
        }

        public static T Random<T>(this T[] list) => list[Util.Random.Next(list.Length)];

        public static T FromRemainder<T>(this T[] array, int amount) => array[amount % array.Length];

        public static T[] Shuffle<T>(this T[] array)
        {
            int length = array.Length;
            T[] newArray = new T[length];
            Array.Copy(array, newArray, length);

            for (int i = 0; i < length; i++)
            {
                int r = Util.Random.Next(i, length);
                T temp = newArray[i];
                newArray[i] = newArray[r];
                newArray[r] = temp;
            }

            return newArray;
        }

        public static bool IsEmpty<T>(this T[] array) => array.Length == 0;
    }
}
