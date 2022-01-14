namespace PettingZoo.UI.Tab.Publisher
{
    public interface IPublishDestination
    {
        string Exchange { get; }
        string RoutingKey { get; }

        string? GetReplyTo(ref string? correlationId);
        void SetExchangeDestination(string exchange, string routingKey);
    }
}
