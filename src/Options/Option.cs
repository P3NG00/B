namespace B.Options
{
    public abstract class Option<T> : IOption where T : Enum
    {
        protected T Stage;
        private bool _running = true;

        public Option(T defaultStage) => this.Stage = defaultStage;

        public abstract void Loop();

        public bool IsRunning() => this._running;

        public virtual void Save() { }

        public virtual void Quit() => this._running = false;
    }
}
