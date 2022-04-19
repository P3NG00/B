namespace B.Utils
{
    public sealed class Togglable
    {
        public bool Active { get; private set; } = false;

        private Action<bool>? _onChange;

        public void SetOnChangeAction(Action<bool> onChange) => _onChange = onChange;

        public void Toggle()
        {
            Active = !Active;

            if (_onChange is not null)
                _onChange(Active);
        }
    }
}
