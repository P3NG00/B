namespace B.Utils
{
    public sealed class Togglable
    {
        public bool Active { get; private set; }

        private Action<bool>? _onChanage;

        public Togglable(Action<bool>? onChange = null) : this(false, onChange) { }

        public Togglable(bool value, Action<bool>? onChange = null)
        {
            Active = value;
            _onChanage = onChange;
        }

        public void Toggle()
        {
            Active = !Active;

            if (_onChanage is not null)
                _onChanage(Active);
        }
    }
}
