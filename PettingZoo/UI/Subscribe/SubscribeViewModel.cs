using System;
using System.Windows.Input;

// TODO validate input

namespace PettingZoo.UI.Subscribe
{
    public class SubscribeViewModel : BaseViewModel
    {
        private string exchange;
        private string routingKey;


        public string Exchange
        {
            get => exchange;
            set => SetField(ref exchange, value);
        }

        public string RoutingKey
        {
            get => routingKey;
            set => SetField(ref routingKey, value);
        }


        public ICommand OkCommand { get; }

        public event EventHandler? OkClick;


        public SubscribeViewModel(SubscribeDialogParams subscribeParams)
        {
            OkCommand = new DelegateCommand(OkExecute, OkCanExecute);
            
            exchange = subscribeParams.Exchange;
            routingKey = subscribeParams.RoutingKey;
        }
        
        
        public SubscribeDialogParams ToModel()
        {
            return new(Exchange, RoutingKey);
        }


        private void OkExecute()
        {
            OkClick?.Invoke(this, EventArgs.Empty);
        }


        private static bool OkCanExecute()
        {
            return true;
        }
    }


    public class DesignTimeSubscribeViewModel : SubscribeViewModel
    {
        public DesignTimeSubscribeViewModel() : base(SubscribeDialogParams.Default)
        {
        }
    }
}
