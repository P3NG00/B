namespace B.Utils
{
    public static class Listb
    {
        public static bool IsEmpty<T>(this List<T> list) => list.Count == 0;

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

        public static bool MoveFirstTo<T>(this List<T> from, List<T> to)
        {
            if (!from.IsEmpty())
            {
                to.Add(from.First());
                from.RemoveAt(0);
                return true;
            }

            return false;
        }
    }
}
