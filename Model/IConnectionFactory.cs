namespace PettingZoo.Model
{
    public interface IConnectionFactory
    {
        IConnection CreateConnection(ConnectionInfo connectionInfo);
    }
}
