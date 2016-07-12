namespace PettingZoo.Connection
{
    public class MockConnectionFactory : IConnectionFactory
    {
        private readonly int interval;

        public MockConnectionFactory(int interval)
        {
            this.interval = interval;
        }

        public IConnection CreateConnection(ConnectionInfo connectionInfo)
        {
            return new MockConnection(interval);
        }
    }
}
