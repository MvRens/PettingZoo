namespace PettingZoo.UI.Tab
{
    public interface ITabHostProvider
    {
        public ITabHost Instance { get; }

        public void SetInstance(ITabHost instance);
    }
}
