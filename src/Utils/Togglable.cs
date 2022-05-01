namespace B.Utils
{
    public sealed class Togglable
    {
        #region Public Variables

        // This must stay a variable for deserialization purposes.
        public bool Active = false;

        #endregion



        #region Private Variables

        private Action<bool>? _onToggle;

        #endregion



        #region Public Methods

        public void SetOnChangeAction(Action<bool> onToggle) => _onToggle = onToggle;

        public void Toggle()
        {
            Active = !Active;
            _onToggle?.Invoke(Active);
        }

        #endregion



        #region Operator Overrides

        public static implicit operator bool(Togglable togglable) => togglable.Active;

        #endregion
    }
}
