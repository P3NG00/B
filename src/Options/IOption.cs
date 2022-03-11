namespace B.Options
{
    public interface IOption
    {
        bool IsRunning { get; }

        void Loop();

        void Quit();
    }
}
