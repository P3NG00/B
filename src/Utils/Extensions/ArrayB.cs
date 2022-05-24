namespace B.Utils.Extensions
{
    public static class ArrayB
    {
        // Randomly shuffles the elements of an array.
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
    }
}
