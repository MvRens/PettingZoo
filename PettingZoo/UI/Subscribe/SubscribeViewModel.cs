using System;
using System.Windows.Input;
using PettingZoo.WPF.ViewModel;

namespace PettingZoo.UI.Subscribe
{
    public class SubscribeViewModel : BaseViewModel
    {
        private string exchange;
        private string routingKey;

        private readonly DelegateCommand okCommand;


        public string Exchange
        {
            get => exchange;
            set => SetField(ref exchange, value, delegateCommandsChanged: new [] { okCommand });
        }

        public string RoutingKey
        {
            get => routingKey;
            set => SetField(ref routingKey, value, delegateCommandsChanged: new[] { okCommand });
        }


        public ICommand OkCommand => okCommand;

        public event EventHandler? OkClick;


        public SubscribeViewModel(SubscribeDialogParams subscribeParams)
        {
            okCommand = new DelegateCommand(OkExecute, OkCanExecute);
            
            exchange = subscribeParams.Exchange;
            routingKey = subscribeParams.RoutingKey;
        }
        
        
        public SubscribeDialogParams ToModel()
        {
            return new SubscribeDialogParams(Exchange, RoutingKey);
        }


        private void OkExecute()
        {
            OkClick?.Invoke(this, EventArgs.Empty);
        }


        private bool OkCanExecute()
        {
            return !string.IsNullOrWhiteSpace(Exchange) && !string.IsNullOrWhiteSpace(RoutingKey);
        }
    }


    public class DesignTimeSubscribeViewModel : SubscribeViewModel
    {
        public DesignTimeSubscribeViewModel() : base(SubscribeDialogParams.Default)
        {
        }
    }
}
