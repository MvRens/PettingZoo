namespace PettingZoo.Connection
{
    public interface IConnectionFactory
    {
        IConnection CreateConnection(ConnectionInfo connectionInfo);
    }
}
