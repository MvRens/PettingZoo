using System;
using System.Linq;
using System.Reflection;
using PettingZoo.Model;
using PettingZoo.View;
using SimpleInjector;

namespace PettingZoo
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
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
                .Where(t => t.IsSubclassOf(typeof(System.Windows.Window)) ||
                            t.IsSubclassOf(typeof(Infrastructure.BaseViewModel))))
            {
                container.Register(type);
            }

            return container;
        }


        private static void RunApplication(Container container) 
        {
            var app = new App();
            var mainWindow = container.GetInstance<MainWindow>();
            app.Run(mainWindow);
        }
    }
}
