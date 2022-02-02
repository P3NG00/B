namespace B.Options
{
    public abstract class Option
    {
        public bool Running { get; private set; } = true;

        public abstract void Loop();

        public virtual void Save() { }

        public virtual void Quit() => this.Running = false;
    }
}
