namespace PettingZoo.Connection
{
    public class MockConnectionInfoBuilder : IConnectionInfoBuilder
    {
        public ConnectionInfo Build()
        {
            return new ConnectionInfo();
        }
    }
}
