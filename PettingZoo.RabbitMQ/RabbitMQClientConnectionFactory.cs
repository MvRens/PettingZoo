using PettingZoo.Core.Connection;

namespace PettingZoo.RabbitMQ
{
    public class RabbitMQClientConnectionFactory : IConnectionFactory
    {
        public IConnection CreateConnection(ConnectionParams connectionParams)
        {
            return new RabbitMQClientConnection(connectionParams);
        }
    }
}
