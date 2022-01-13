namespace B.Options
{
    public abstract class Option
    {
        public bool IsRunning { get; private set; } = true;

        public abstract void Loop();

        public virtual void Save() { }

        public void Quit() => this.IsRunning = false;
    }
}
