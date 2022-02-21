namespace B.Utils
{
    [Serializable]
    public sealed class Togglable
    {
        private bool _value;

        public bool Active => this._value;

        public Togglable() : this(false) { }

        public Togglable(bool value) => this._value = value;

        public void Toggle() => this._value = !this._value;
    }
}