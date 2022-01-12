using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PettingZoo.Core.Connection;
using PettingZoo.Core.ExportImport;


namespace PettingZoo.Tapeti.Export
{
    public class TapetiCmdExportFormat : BaseTapetiCmdExportImportFormat, IExportFormat
    {
        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };


        public async Task Export(Stream stream, IEnumerable<ReceivedMessageInfo> messages, CancellationToken cancellationToken)
        {
            await using var exportFile = new StreamWriter(stream, Encoding.UTF8);

            foreach (var message in messages)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var serializableMessage = new SerializableMessage
                {
                    Exchange = message.Exchange,
                    RoutingKey = message.RoutingKey,
                    Properties = new SerializableMessageProperties
                    {
                        AppId = message.Properties.AppId,
                        ContentEncoding = message.Properties.ContentEncoding,
                        ContentType = message.Properties.ContentType,
                        CorrelationId = message.Properties.CorrelationId,
                        DeliveryMode = message.Properties.DeliveryMode switch
                        {
                            MessageDeliveryMode.Persistent => 2,
                            _ => 1
                        },
                        Expiration = message.Properties.Expiration,
                        Headers = message.Properties.Headers.Count > 0 ? message.Properties.Headers.ToDictionary(p => p.Key, p => p.Value) : null,
                        MessageId = message.Properties.MessageId,
                        Priority = message.Properties.Priority,
                        ReplyTo = message.Properties.ReplyTo,
                        Timestamp = message.Properties.Timestamp.HasValue ? new DateTimeOffset(message.Properties.Timestamp.Value).ToUnixTimeSeconds() : null,
                        Type = message.Properties.Type,
                        UserId = message.Properties.UserId
                    }
                };


                var useRawBody = true;
                if (message.Properties.ContentType == @"application/json")
                {
                    try
                    {
                        if (JToken.Parse(Encoding.UTF8.GetString(message.Body)) is JObject jsonBody)
                        {
                            serializableMessage.Body = jsonBody;
                            useRawBody = false;
                        }
                    }
                    catch
                    {
                        // Use raw body
                    }
                }

                if (useRawBody)
                    serializableMessage.RawBody = message.Body;

                var serialized = JsonConvert.SerializeObject(serializableMessage, SerializerSettings);
                await exportFile.WriteLineAsync(serialized);
            }
        }
    }
}
