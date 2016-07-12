using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using Newtonsoft.Json;
using PettingZoo.Connection;
using PettingZoo.Infrastructure;
using PettingZoo.View;
using PettingZoo.ViewModel;
using SimpleInjector;

namespace PettingZoo
{
    public partial class App
    {
        public void ApplicationStartup(object sender, StartupEventArgs e)
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

            container.RegisterSingleton(() => new UserSettings(new AppDataSettingsSerializer("Settings.json")));

            container.Register<IConnectionFactory, RabbitMQClientConnectionFactory>();
            container.Register<IConnectionInfoBuilder, WindowConnectionInfoBuilder>();

            //container.Register<IConnectionFactory>(() => new MockConnectionFactory(10));
            //container.Register<IConnectionInfoBuilder, MockConnectionInfoBuilder>();


            container.Register<MainWindow>();
            container.Register<MainViewModel>();

            // Note: don't run Verify! It'll create a MainWindow which will then become
            // Application.Current.MainWindow and prevent the process from shutting down.

            return container;
        }


        private static void RunApplication(Container container) 
        {
            var mainWindow = container.GetInstance<MainWindow>();
            mainWindow.Closed += (sender, args) => container.Dispose();

            mainWindow.Show();
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
                Directory.CreateDirectory(path);
                File.WriteAllText(fullPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
            }


            private T GetProductInfo<T>()
            {
                var attributes = GetType().Assembly.GetCustomAttributes(typeof(T), true);
                if (attributes.Length == 0)
                    throw new Exception("Missing product information in assembly");

                return (T)attributes[0];
            }
        }
    }
}
