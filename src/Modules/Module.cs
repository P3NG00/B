using B.Utils;

namespace B.Modules
{
    public abstract class Module<T> : IModule where T : Enum
    {
        #region Public Properties

        // Current stage of the module.
        public T Stage { get; private set; }
        // Status of the module.
        public bool IsRunning { get; private set; } = true;

        #endregion



        #region Constructors

        // Creates a new module instance.
        public Module(T defaultStage) => Stage = defaultStage;

        #endregion



        #region Protected Methods

        // Set the stage of the module.
        protected virtual void SetStage(T stage)
        {
            // Window is cleared when stage is changed because no stages need visual info from the last
            Window.Clear();
            // Update stage
            Stage = stage;
        }

        #endregion



        #region Public Methods

        // Main module loop.
        public abstract void Loop();

        // Stops the module.
        public virtual void Quit() => IsRunning = false;

        #endregion
    }
}
