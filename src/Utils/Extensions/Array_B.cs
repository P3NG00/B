namespace B.Utils.Extensions
{
    public static class ArrayB
    {
        public static void Shuffle<T>(this T[] array)
        {
            Random random = Util.Random;

            for (int i = 0; i < array.Length; i++)
            {
                int j = random.Next(i, array.Length);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
    }
}
