using System;
using System.Windows.Input;
using AutoMapper;
using PettingZoo.Infrastructure;
using PettingZoo.Model;

namespace PettingZoo.ViewModel
{
    public class ConnectionViewModel
    {
        private static IMapper modelMapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ConnectionInfo, ConnectionViewModel>();
            cfg.CreateMap<ConnectionViewModel, ConnectionInfo>();
        }).CreateMapper();


        private readonly DelegateCommand okCommand;

        
        public string Host { get; set; }
        public string VirtualHost { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string Exchange { get; set; }
        public string RoutingKey { get; set;  }


        public ICommand OkCommand { get { return okCommand; } }

        public event EventHandler CloseWindow;


        public ConnectionViewModel()
        {
            okCommand = new DelegateCommand(OkExecute, OkCanExecute);
        }


        public ConnectionViewModel(ConnectionInfo model) : this()
        {
            modelMapper.Map(model, this);
        }


        public ConnectionInfo ToModel()
        {
            return modelMapper.Map<ConnectionInfo>(this);
        }


        private void OkExecute()
        {
            if (CloseWindow != null)
                CloseWindow(this, EventArgs.Empty);
        }


        private bool OkCanExecute()
        {
            return true;
        }
    }
}
