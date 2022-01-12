namespace B.Options
{
    public abstract class Option
    {
        public bool IsRunning { get { return this._running; } }
        private bool _running = true;

        public virtual void Save() { }

        public void Quit() { this._running = false; }

        public abstract void Loop();
    }
}
