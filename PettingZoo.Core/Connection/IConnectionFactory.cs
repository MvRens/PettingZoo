namespace PettingZoo.Core.Connection
{
    public interface IConnectionFactory
    {
        IConnection CreateConnection(ConnectionParams connectionInfo);
    }
}
