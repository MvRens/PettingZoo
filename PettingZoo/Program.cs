using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using Newtonsoft.Json;
using PettingZoo.Core.Connection;
using PettingZoo.RabbitMQ;
using PettingZoo.Settings;
using PettingZoo.UI.Connection;
using PettingZoo.UI.Main;
using PettingZoo.UI.Subscribe;
using PettingZoo.UI.Tab;
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

            container.RegisterSingleton(() => new UserSettings(new AppDataSettingsSerializer("Settings.json")));

            container.Register<IConnectionFactory, RabbitMQClientConnectionFactory>();
            container.Register<IConnectionDialog, WindowConnectionDialog>();
            container.Register<ISubscribeDialog, WindowSubscribeDialog>();
            container.Register<ITabFactory, ViewTabFactory>();

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
            catch (Exception)
            {
                // TODO Log the exception and exit
            }
        }


        private class AppDataSettingsSerializer : IUserSettingsSerializer
        {
            private readonly string path;
            private readonly string fullPath;


            public AppDataSettingsSerializer(string filename)
            {
                var companyName = GetProductInfo<AssemblyCompanyAttribute>().Company;
                var productName = GetProductInfo<AssemblyProductAttribute>().Product;

                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    companyName, productName);
                fullPath = Path.Combine(path, filename);
            }


            public void Read(UserSettings settings)
            {
                if (File.Exists(fullPath))
                    JsonConvert.PopulateObject(File.ReadAllText(fullPath), settings);
            }


            public void Write(UserSettings settings)
            {
                _ = Directory.CreateDirectory(path);
                File.WriteAllText(fullPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
            }


            private T GetProductInfo<T>()
            {
                var attributes = GetType().Assembly.GetCustomAttributes(typeof(T), true);
                return attributes.Length == 0
                    ? throw new Exception("Missing product information in assembly")
                    : (T) attributes[0];
            }
        }
    }
}