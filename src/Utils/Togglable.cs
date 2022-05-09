using Newtonsoft.Json;

namespace B.Utils
{
    public sealed class Togglable
    {
        #region Public Variables

        [JsonIgnore] public bool Enabled => _enabled;

        #endregion



        #region Private Variables

        [JsonProperty] private bool _enabled;
        private Action<bool>? _onToggle;

        #endregion



        #region Constructors

        public Togglable(bool defaultValue = false) => _enabled = defaultValue;

        #endregion



        #region Public Methods

        public void SetOnChangeAction(Action<bool> onToggle) => _onToggle = onToggle;

        public void SetOnChangeAction(Action onToggle) => SetOnChangeAction(b => onToggle());

        public void Toggle()
        {
            _enabled = !_enabled;
            _onToggle?.Invoke(_enabled);
        }

        #endregion



        #region Operator Overrides

        public static implicit operator bool(Togglable togglable) => togglable.Enabled;

        #endregion
    }
}
