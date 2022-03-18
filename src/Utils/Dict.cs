namespace B.Utils
{
    public sealed class Dict<T, V>
    {
        private (T, V)[] _pairs = new (T, V)[0];

        public int Length => _pairs.Length;

        public Dict(params (T, V)[] pairs) => _pairs = pairs;

        public void Add(T t, V v)
        {
            (T, V)[] newPairs = new (T, V)[_pairs.Length + 1];
            Array.Copy(_pairs, newPairs, _pairs.Length);
            newPairs[_pairs.Length] = new(t, v);
            _pairs = newPairs;
        }

        public void Remove(T t)
        {
            (T, V)[] newPairs = new (T, V)[_pairs.Length - 1];
            int index = 0;

            for (int i = 0; i < _pairs.Length; i++)
                if (!_pairs[i].Item1!.Equals(t))
                    newPairs[index++] = _pairs[i];

            _pairs = newPairs;
        }

        public bool ContainsKey(T t)
        {
            foreach ((T, V) pair in _pairs)
                if (pair.Item1!.Equals(t))
                    return true;

            return false;
        }

        public V this[T t]
        {
            get => _pairs[GetIndex(t)].Item2!;
            set => _pairs[GetIndex(t)].Item2 = value;
        }

        public (T, V) this[int index] => _pairs[index];

        private int GetIndex(T t)
        {
            for (int i = 0; i < _pairs.Length; i++)
                if (_pairs[i].Item1!.Equals(t))
                    return i;

            throw new ArgumentException("Key not found");
        }
    }
}
