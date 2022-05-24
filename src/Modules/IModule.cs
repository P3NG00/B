namespace B.Modules
{
    public interface IModule
    {
        #region Properties

        // Status of the module.
        bool IsRunning { get; }

        #endregion



        #region Methods

        // Main module loop.
        void Loop();

        // Stops the module.
        void Quit();

        #endregion
    }
}
