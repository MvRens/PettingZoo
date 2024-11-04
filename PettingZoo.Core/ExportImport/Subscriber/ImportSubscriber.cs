using System;
using System.Collections.Generic;
using System.IO;
using PettingZoo.Core.Connection;

namespace PettingZoo.Core.ExportImport.Subscriber
{
    public class ImportSubscriber : ISubscriber
    {
        private readonly IReadOnlyList<ReceivedMessageInfo> messages;

        public string? QueueName { get; }
        public string? Exchange => null;
        public string? RoutingKey => null;

        #pragma warning disable CS0067 // "The event ... is never used" - it's part of the interface so it's required.
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        #pragma warning restore CS0067


        public ImportSubscriber(string filename, IReadOnlyList<ReceivedMessageInfo> messages)
        {
            QueueName = Path.GetFileName(filename);
            this.messages = messages;
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
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
