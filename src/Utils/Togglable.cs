namespace B.Utils
{
    [Serializable]
    public sealed class Togglable
    {
        public bool Active = false;

        public Togglable() : this(false) { }

        public Togglable(bool value) => this.Active = value;

        public void Toggle() => Util.ToggleBool(ref this.Active);
    }
}
