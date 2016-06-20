using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PettingZoo.Model
{
    public class RabbitMQClientConnection : IConnection
    {
        private readonly CancellationTokenSource timer;

        public RabbitMQClientConnection(ConnectionInfo connectionInfo)
        {
            timer = new CancellationTokenSource();
            var token = timer.Token;

            Task.Run(() =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                        break;

                    if (MessageReceived != null)
                        MessageReceived(null, new MessageReceivedEventArgs(new MessageInfo()
                        {
                            RoutingKey = "test",
                            Body = Encoding.UTF8.GetBytes("{ \"hello\": \"world\" }"),
                            Properties = new Dictionary<string, string>
                            {
                                { "content-type", "application/json" },
                                { "classType", "LEF.Messaging.Internal.ActieNewMessage" }
                            }
                        }));
                    Thread.Sleep(1000);
                }
            }, token);
        }


        public void Dispose()
        {
            timer.Cancel();
        }


        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
