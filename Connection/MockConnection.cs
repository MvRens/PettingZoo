using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PettingZoo.Connection
{
    public class MockConnection : IConnection
    {
        private Timer messageTimer;
        private bool connected;

        public MockConnection(int interval)
        {
            messageTimer = new Timer(state =>
            {
                if (!connected)
                {
                    StatusChanged(this, new StatusChangedEventArgs(ConnectionStatus.Connected, "Mock"));
                    connected = true;
                }

                MessageReceived(this, new MessageReceivedEventArgs(new MessageInfo
                {
                    Exchange = "mock",
                    RoutingKey = "mock.mock",
                    Timestamp = DateTime.Now,
                    Body = Encoding.UTF8.GetBytes("Mock!"),
                    Properties = new Dictionary<string, string>
                    {
                        { "mock", "mock" }
                    }}));
            }, null, interval, interval);
        }


        public void Dispose()
        {
            if (messageTimer != null)
            {
                messageTimer.Dispose();
                messageTimer = null;
            }
        }


        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
