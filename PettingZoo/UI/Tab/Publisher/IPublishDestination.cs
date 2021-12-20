namespace PettingZoo.UI.Tab.Publisher
{
    public interface IPublishDestination
    {
        string Exchange { get; }
        string RoutingKey { get; }

        string? GetReplyTo();
    }
}
