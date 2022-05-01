namespace B.Options
{
    public interface IOption
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
