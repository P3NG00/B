namespace B.Utils
{
    [Serializable]
    public sealed class Togglable
    {
        public bool Active = false;

        public Togglable() : this(false) { }

        public Togglable(bool value) => Active = value;

        public void Toggle() => Active = !Active;

        // TODO consider settable OnToggle action to invoke when toggled
    }
}
