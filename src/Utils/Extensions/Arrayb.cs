namespace B.Utils.Extensions
{
    public static class ArrayB
    {
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
    }
}
