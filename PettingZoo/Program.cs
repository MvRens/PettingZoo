using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using PettingZoo.Core.Connection;
using PettingZoo.Core.Settings;
using PettingZoo.RabbitMQ;
using PettingZoo.Settings.LiteDB;
using PettingZoo.UI.Connection;
using PettingZoo.UI.Main;
using PettingZoo.UI.Subscribe;
using SimpleInjector;

namespace PettingZoo
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            // WPF defaults to US for date formatting in bindings, this fixes it
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            var container = Bootstrap();
            RunApplication(container);
        }


        private static Container Bootstrap()
        {
            var container = new Container();
            
            // See comments in RunApplication
            container.Options.EnableAutoVerification = false;

            container.Register<IConnectionFactory, RabbitMQClientConnectionFactory>();
            container.Register<IConnectionDialog, WindowConnectionDialog>();
            container.Register<ISubscribeDialog, WindowSubscribeDialog>();
            container.Register<IConnectionSettingsRepository, LiteDBConnectionSettingsRepository>();

            container.Register<MainWindow>();
            
            return container;
        }


        private static void RunApplication(Container container)
        {
            try
            {
                var app = new App();
                app.InitializeComponent();

                #if DEBUG
                // Verify container after initialization to prevent issues loading the resource dictionaries
                container.Verify();

                // This causes the MainWindow and Windows properties to be populated however, which we don't want
                // because then the app does not close properly when using OnMainWindowClose, so clean up the mess
                app.MainWindow = null;
                foreach (var window in app.Windows)
                    ((Window)window).Close();
                
                // All this is the reason we only perform verification in debug builds
                #endif

                var mainWindow = container.GetInstance<MainWindow>();
                _ = app.Run(mainWindow);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Fatal exception: {e.Message}", @"PettingZoo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}