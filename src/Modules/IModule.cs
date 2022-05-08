namespace B.Modules
{
    public interface IModule
    {
        #region Properties

        bool IsRunning { get; }

        #endregion



        #region Methods

        void Loop();

        void Quit();

        #endregion
    }
}
