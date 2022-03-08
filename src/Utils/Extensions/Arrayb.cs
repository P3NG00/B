namespace B.Utils
{
    public static class Arrayb
    {
        public static T[] Add<T>(this T[] array, params T[] items)
        {
            T[] newArray = new T[array.Length + items.Length];
            Array.Copy(array, newArray, array.Length);
            Array.Copy(items, 0, newArray, array.Length, items.Length);
            return newArray;
        }

        public static T[] Add<T>(this T[] array, T item, int index)
        {
            T[] newArray = new T[array.Length + 1];
            Array.Copy(array, newArray, index);
            newArray[index] = item;
            Array.Copy(array, index, newArray, index + 1, array.Length - index);
            return newArray;
        }

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

        public static T[] Remove<T>(this T[] array, int index)
        {
            T[] newArray = new T[array.Length - 1];
            Array.Copy(array, newArray, index);
            Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
            return newArray;
        }

        public static bool Contains<T>(this T[] array, T item)
        {
            foreach (T currentItem in array)
                if (currentItem is not null && currentItem.Equals(item))
                    return true;

            return false;
        }

        // TODO go through and make this referenced
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            foreach (T item in array)
                action(item);
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

        public static void Shuffle<T>(this T[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                int j = Util.Random.Next(i, array.Length);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        public static T[] Copy<T>(this T[] array)
        {
            T[] newArray = new T[array.Length];
            Array.Copy(array, newArray, array.Length);
            return newArray;
        }

        public static bool IsEmpty<T>(this T[] array) => array.Length == 0;
    }
}
