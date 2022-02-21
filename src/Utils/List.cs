using System.Collections;

namespace B.Utils
{
    [Serializable]
    public sealed class List<T>
    {
        public T[] Items = new T[0];

        public int Length => this.Items.Length;
        public bool IsEmpty => this.Length == 0;

        public List() { }

        public List(params T[] items) => this.Items = items;

        public void Add(params T[] t)
        {
            T[] newItems = new T[this.Items.Length + t.Length];
            Array.Copy(this.Items, newItems, this.Items.Length);
            Array.Copy(t, 0, newItems, this.Items.Length, t.Length);
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

        public List<U> GetSubList<U>(Func<T, U> getValue)
        {
            List<U> list = new List<U>();

            foreach (T item in this.Items)
                list.Add(getValue(item));

            return list;
        }

        public IEnumerator GetEnumerator() => this.Items.GetEnumerator();

        public T this[int index] => this.Items[index];
    }
}
