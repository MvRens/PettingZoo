using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PettingZoo.Core.Connection;

namespace PettingZoo.Core.ExportImport
{
    public class ImportSubscriber : ISubscriber
    {
        private readonly IReadOnlyList<ReceivedMessageInfo> messages;

        public string? QueueName { get; }
        public string? Exchange => null;
        public string? RoutingKey => null;
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;


        public ImportSubscriber(string filename, IReadOnlyList<ReceivedMessageInfo> messages)
        {
            QueueName = Path.GetFileName(filename);
            this.messages = messages;
        }


        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return default;
        }


        public IEnumerable<ReceivedMessageInfo> GetInitialMessages()
        {
            return messages;
        }


        public void Start()
        {
        }
    }
}
