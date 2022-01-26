using System.Collections;

namespace B.Utils
{
    [Serializable]
    public sealed class List<T>
    {
        public T[] Items = new T[0];

        public int Length => this.Items.Length;

        public void Add(T t)
        {
            T[] newItems = new T[this.Items.Length + 1];
            Array.Copy(this.Items, newItems, this.Items.Length);
            newItems[this.Items.Length] = t!;
            this.Items = newItems;
        }

        public void Remove(T t)
        {
            T[] newItems = new T[this.Items.Length - 1];
            int index = 0;

            for (int i = 0; i < this.Items.Length; i++)
            {
                if (this.Items[i]!.Equals(t))
                    continue;

                newItems[index] = this.Items[i];
                index++;
            }

            this.Items = newItems;
        }

        public bool Contains(T t)
        {
            foreach (T item in this.Items)
                if (item!.Equals(t))
                    return true;

            return false;
        }

        public IEnumerator GetEnumerator() => this.Items.GetEnumerator();

        public T this[int index] => this.Items[index];
    }
}
