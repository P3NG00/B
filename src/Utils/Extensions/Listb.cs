namespace B.Utils.Extensions
{
    public static class ListB
    {
        public static T RemoveRandom<T>(this List<T> list)
        {
            T item = list.Random();
            list.Remove(item);
            return item;
        }

        public static void Shuffle<T>(this List<T> list)
        {
            Random random = Util.Random;

            for (int i = 0; i < list.Count; i++)
            {
                int j = random.Next(i, list.Count);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        public static void Sort<T>(this List<T> list, Func<T, float> sortBy) => list.Sort((a, b) => (int)(sortBy(a) - (sortBy(b))));
    }
}
