using System.Collections;

namespace B.Utils
{
    [Serializable]
    public sealed class List<T>
    {
        private T[] _items = new T[0];

        public int Count => this._items.Length;

        public void Add(T t)
        {
            T[] newItems = new T[this._items.Length + 1];
            Array.Copy(this._items, newItems, this._items.Length);
            newItems[this._items.Length] = t!;
            this._items = newItems;
        }

        public void Remove(T t)
        {
            T[] newItems = new T[this._items.Length - 1];
            int index = 0;

            for (int i = 0; i < this._items.Length; i++)
            {
                if (this._items[i]!.Equals(t))
                    continue;

                newItems[index] = this._items[i];
                index++;
            }

            this._items = newItems;
        }

        public bool Contains(T t)
        {
            foreach (T item in this._items)
                if (item!.Equals(t))
                    return true;

            return false;
        }

        public IEnumerator GetEnumerator() => this._items.GetEnumerator();

        public T this[int index] => this._items[index];
    }
}
