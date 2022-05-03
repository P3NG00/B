using B.Utils;

namespace B.Options
{
    public abstract class Option<T> : IOption where T : Enum
    {
        #region Public Properties

        public T Stage { get; private set; }
        public bool IsRunning { get; private set; } = true;

        #endregion



        #region Constructors

        public Option(T defaultStage) => Stage = defaultStage;

        #endregion



        #region Protected Methods

        protected virtual void SetStage(T stage)
        {
            Window.Clear();
            Stage = stage;
        }

        #endregion



        #region Public Methods

        public abstract void Loop();

        public virtual void Save() { }

        public virtual void Quit() => IsRunning = false;

        #endregion
    }
}
