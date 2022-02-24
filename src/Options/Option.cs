namespace B.Options
{
    public abstract class Option<T> : IOption where T : Enum
    {
        protected T Stage { get; private set; }
        private bool _running = true;

        public Option(T defaultStage) => this.Stage = defaultStage;

        protected void SetStage(T stage) => this.Stage = stage;

        public abstract void Loop();

        public bool IsRunning() => this._running;

        public virtual void Save() { }

        public virtual void Quit() => this._running = false;
    }
}
