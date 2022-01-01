namespace PettingZoo.UI.Subscribe
{
    public interface ISubscribeDialog
    {
        SubscribeDialogParams? Show(SubscribeDialogParams? defaultParams = null);
    }


    public class SubscribeDialogParams
    {
        public string Exchange { get; }
        public string RoutingKey { get; }


        public static SubscribeDialogParams Default { get; } = new("", "#");


        public SubscribeDialogParams(string exchange, string routingKey)
        {
            Exchange = exchange;
            RoutingKey = routingKey;
        }
    }
}
