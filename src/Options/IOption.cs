namespace B.Options
{
    public interface IOption
    {
        public abstract void Loop();

        public abstract bool IsRunning();

        public abstract void Quit();
    }
}
