namespace PettingZoo.Model
{
    public class RabbitMQClientConnectionFactory : IConnectionFactory
    {
        public IConnection CreateConnection(ConnectionInfo connectionInfo)
        {
            return new RabbitMQClientConnection(connectionInfo);
        }
    }
}
