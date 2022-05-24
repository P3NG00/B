using Newtonsoft.Json;

namespace B.Utils
{
    public sealed class Togglable
    {
        #region Public Variables

        // Get the state of the Togglable.
        [JsonIgnore] public bool Enabled => _enabled;

        #endregion



        #region Private Variables

        // State of the Togglable.
        [JsonProperty] private bool _enabled;
        // Action to invoke upon toggling.
        [JsonIgnore] private Action<bool>? _onToggle;

        #endregion



        #region Constructors

        // Creates a Togglable with the specified value.
        public Togglable(bool firstStatus) => _enabled = firstStatus;

        #endregion



        #region Public Methods

        // Sets the action to invoke upon toggling.
        public void SetOnChangeAction(Action<bool> onToggle) => _onToggle = onToggle;

        // Sets the action to invoke upon toggling.
        public void SetOnChangeAction(Action onToggle) => SetOnChangeAction(b => onToggle());

        // Toggles the state of the Togglable. Invokes action if set.
        public void Toggle()
        {
            _enabled = !_enabled;
            _onToggle?.Invoke(_enabled);
        }

        #endregion



        #region Operator Overrides

        // Implicitly uses a Togglable as a bool by its state.
        public static implicit operator bool(Togglable togglable) => togglable.Enabled;

        #endregion
    }
}
