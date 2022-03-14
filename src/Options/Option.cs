namespace B.Options
{
    public abstract class Option<T> : IOption where T : Enum
    {
        protected T Stage { get; private set; }

        public bool IsRunning { get; private set; } = true;

        public Option(T defaultStage) => Stage = defaultStage;

        protected virtual void SetStage(T stage) => Stage = stage;

        public abstract void Loop();

        public virtual void Save() { }

        public virtual void Quit() => IsRunning = false;
    }
}
