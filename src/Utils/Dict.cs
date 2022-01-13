namespace B.Utils
{
    [Serializable]
    public sealed class Dict<T, V>
    {
        private Pair<T, V>[] _pairs = new Pair<T, V>[0];

        public int Count => this._pairs.Length;

        public void Add(T t, V v)
        {
            Pair<T, V>[] newPairs = new Pair<T, V>[this._pairs.Length + 1];
            Array.Copy(this._pairs, newPairs, this._pairs.Length);
            newPairs[this._pairs.Length] = new Pair<T, V>(t, v);
            this._pairs = newPairs;
        }

        public void Remove(T t)
        {
            Pair<T, V>[] newPairs = new Pair<T, V>[this._pairs.Length - 1];
            int index = 0;

            for (int i = 0; i < this._pairs.Length; i++)
                if (!this._pairs[i].Item1!.Equals(t))
                    newPairs[index++] = this._pairs[i];

            this._pairs = newPairs;
        }

        public bool ContainsKey(T t)
        {
            foreach (Pair<T, V> pair in this._pairs)
                if (pair.Item1!.Equals(t))
                    return true;

            return false;
        }

        public V this[T t]
        {
            get => this._pairs[this.GetIndex(t)].Item2;
            set => this._pairs[this.GetIndex(t)].Item2 = value;
        }

        private int GetIndex(T t)
        {
            for (int i = 0; i < this._pairs.Length; i++)
                if (this._pairs[i].Item1!.Equals(t))
                    return i;

            throw new ArgumentException("Key not found");
        }
    }
}
