using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PettingZoo.Core.Settings;
using PettingZoo.UI.Main;
using SimpleInjector;
using Point = System.Windows.Point;

namespace PettingZoo
{
    public partial class App
    {
        private readonly Container container;


        public App()
        {
            throw new InvalidOperationException("Default main should not be used");
        }


        public App(Container container)
        {
            this.container = container;
        }


        protected override async void OnStartup(StartupEventArgs e)
        {
            var uitSettingsRepository = container.GetInstance<IUISettingsRepository>();
            var position = await uitSettingsRepository.GetMainWindowPosition();

            var mainWindow = container.GetInstance<MainWindow>();

            if (position != null)
            {
                var positionBounds = new Rect(
                    new Point(position.Left, position.Top), 
                    new Point(position.Left + position.Width, position.Top + position.Height));

                if (InScreenBounds(positionBounds))
                {
                    mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    mainWindow.Top = positionBounds.Top;
                    mainWindow.Left = positionBounds.Left;
                    mainWindow.Width = positionBounds.Width;
                    mainWindow.Height = positionBounds.Height;
                }

                mainWindow.WindowState = position.Maximized ? WindowState.Maximized : WindowState.Normal;
            }

            mainWindow.Closing += (_, _) =>
            {
                var newPosition = new MainWindowPositionSettings(
                    (int)mainWindow.RestoreBounds.Top,
                    (int)mainWindow.RestoreBounds.Left,
                    (int)mainWindow.RestoreBounds.Width,
                    (int)mainWindow.RestoreBounds.Height,
                    mainWindow.WasMaximized);

                Task.Run(() => uitSettingsRepository.StoreMainWindowPosition(newPosition));
            };

            mainWindow.Show();
        }


        private static bool InScreenBounds(Rect bounds)
        {
            var boundsRectangle = new Rectangle((int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height);

            // There doesn't appear to be any way to get this information other than from System.Windows.From/PInvoke at the time of writing
            return System.Windows.Forms.Screen.AllScreens.Any(screen => screen.Bounds.IntersectsWith(boundsRectangle));
        }


        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _ = MessageBox.Show($"Unhandled exception: {e.Exception.Message}", "Petting Zoo - Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
