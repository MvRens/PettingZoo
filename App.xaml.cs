using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using PettingZoo.Model;
using PettingZoo.View;
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

            container.Register<IConnectionFactory, RabbitMQClientConnectionFactory>();
            container.Register<IConnectionInfoBuilder, WindowConnectionInfoBuilder>();

            // Automatically register all Window and BaseViewModel descendants
            foreach (var type in Assembly.GetExecutingAssembly().GetExportedTypes()
                .Where(t => t.IsSubclassOf(typeof(Window)) ||
                            t.IsSubclassOf(typeof(Infrastructure.BaseViewModel))))
            {
                container.Register(type);
            }

            return container;
        }


        private static void RunApplication(Container container) 
        {
            var mainWindow = container.GetInstance<MainWindow>();
            mainWindow.Closed += (sender, args) => container.Dispose();

            mainWindow.Show();
        }
    }
}
