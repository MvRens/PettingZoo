using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PettingZoo.Core.Connection;
using PettingZoo.Core.ExportImport.Subscriber;

namespace PettingZoo.Tapeti.Export
{
    public class TapetiCmdImportFormat : BaseTapetiCmdExportImportFormat, IImportFormat
    {
        private const int DefaultBufferSize = 1024;


        public async Task<IReadOnlyList<ReceivedMessageInfo>> Import(Stream stream, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            using var reader = new StreamReader(stream, Encoding.UTF8, true, DefaultBufferSize, true);

            var messages = new List<ReceivedMessageInfo>();

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var serialized = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(serialized))
                    continue;

                var serializableMessage = JsonConvert.DeserializeObject<SerializableMessage>(serialized);
                if (serializableMessage == null)
                    continue;

                var body = serializableMessage.Body != null 
                    ? Encoding.UTF8.GetBytes(serializableMessage.Body.ToString(Formatting.Indented)) 
                    : serializableMessage.RawBody;

                var messageTimestamp = serializableMessage.Properties?.Timestamp != null
                    ? DateTimeOffset.FromUnixTimeSeconds(serializableMessage.Properties.Timestamp.Value).LocalDateTime
                    : now;

                messages.Add(new ReceivedMessageInfo(
                    serializableMessage.Exchange ?? "", 
                    serializableMessage.RoutingKey ?? "", 
                    body ?? Array.Empty<byte>(),

                    // IReadOnlyDictionary is not compatible with IDictionary? wow.
                    new MessageProperties(serializableMessage.Properties?.Headers?.ToDictionary(p => p.Key, p => p.Value))
                    {
                        AppId = serializableMessage.Properties?.AppId,
                        ContentEncoding = serializableMessage.Properties?.ContentEncoding,
                        ContentType = serializableMessage.Properties?.ContentType,
                        CorrelationId = serializableMessage.Properties?.CorrelationId,
                        DeliveryMode = serializableMessage.Properties?.DeliveryMode switch
                        {
                            2 => MessageDeliveryMode.Persistent,
                            _ => MessageDeliveryMode.NonPersistent
                        },
                        Expiration = serializableMessage.Properties?.Expiration,
                        MessageId = serializableMessage.Properties?.MessageId,
                        Priority = serializableMessage.Properties?.Priority,
                        ReplyTo = serializableMessage.Properties?.ReplyTo,
                        Timestamp = messageTimestamp,
                        Type = serializableMessage.Properties?.Type,
                        UserId = serializableMessage.Properties?.UserId
                    },
                    messageTimestamp));
            }

            return messages;
        }
    }
}
