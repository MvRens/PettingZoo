using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using PettingZoo.Core.Connection;
using PettingZoo.Core.Generator;
using PettingZoo.Core.Settings;
using PettingZoo.RabbitMQ;
using PettingZoo.Settings.LiteDB;
using PettingZoo.Tapeti;
using PettingZoo.UI.Connection;
using PettingZoo.UI.Main;
using PettingZoo.UI.Subscribe;
using Serilog;
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

            try
            {
                var logger = CreateLogger();
                try
                {
                    logger.Verbose("Bootstrapping...");
                    var container = Bootstrap(logger);

                    logger.Verbose("Running application...");
                    RunApplication(container, logger);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Unhandled exception");
                    throw;
                }
                finally
                {
                    logger.Verbose("Shutting down");
                }
            }
            catch (Exception e)
            {
                _ = MessageBox.Show($"Unhandled exception: {e.Message}", "Petting Zoo - Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private static ILogger CreateLogger()
        {
            var logPath = Path.Combine(PettingZooPaths.LogPath, @"PettingZoo.log");

            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }


        private static Container Bootstrap(ILogger logger)
        {
            var container = new Container();
            
            // See comments in RunApplication
            container.Options.EnableAutoVerification = false;

            container.RegisterInstance(logger);
            container.Register<IConnectionFactory, RabbitMQClientConnectionFactory>();
            container.Register<IConnectionDialog, WindowConnectionDialog>();
            container.Register<ISubscribeDialog, WindowSubscribeDialog>();
            container.Register<IConnectionSettingsRepository, LiteDBConnectionSettingsRepository>();
            container.Register<IUISettingsRepository, LiteDBUISettingsRepository>();
            container.Register<IExampleGenerator, TapetiClassLibraryExampleGenerator>();

            container.Register<MainWindow>();
            
            return container;
        }


        private static void RunApplication(Container container, ILogger logger)
        {
            var app = new App(container, logger);
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
            
            app.Run();
        }
    }
}