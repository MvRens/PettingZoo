using System.Threading.Tasks;

namespace PettingZoo.Core.Settings
{
    public interface IUISettingsRepository
    {
        Task<MainWindowPositionSettings?> GetMainWindowPosition();
        Task StoreMainWindowPosition(MainWindowPositionSettings settings);
    }


    public class MainWindowPositionSettings
    {
        public int Top { get; }
        public int Left { get; }
        public int Width { get; }
        public int Height { get; }
        public bool Maximized { get; }


        public MainWindowPositionSettings(int top, int left, int width, int height, bool maximized)
        {
            Top = top;
            Left = left;
            Width = width;
            Height = height;
            Maximized = maximized;
        }
    }
}
