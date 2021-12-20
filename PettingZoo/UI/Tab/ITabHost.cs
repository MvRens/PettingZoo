namespace PettingZoo.UI.Tab
{
    public interface ITabHost
    {
        void AddTab(ITab tab);

        void DockTab(ITab tab);
        void UndockedTabClosed(ITab tab);
    }
}
