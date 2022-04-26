namespace B.Utils
{
    public sealed class Togglable
    {
        // This must stay a variable for deserialization purposes.
        public bool Active = false;

        private Action<bool>? _onToggle;

        public void SetOnChangeAction(Action<bool> onToggle) => _onToggle = onToggle;

        public void Toggle()
        {
            Active = !Active;
            _onToggle?.Invoke(Active);
        }

        public static implicit operator bool(Togglable togglable) => togglable.Active;
    }
}
