using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PettingZoo.Core.Connection;
using PettingZoo.Core.Export;

namespace PettingZoo.Tapeti.Export
{
    public class TapetiCmdExportFormat : IExportFormat
    {
        public string Filter => TapetiCmdExportStrings.TapetiCmdFilter;


        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };


        public async Task Export(Stream stream, IEnumerable<ReceivedMessageInfo> messages)
        {
            await using var exportFile = new StreamWriter(stream, Encoding.UTF8);

            foreach (var message in messages)
            {
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


    // It would be nicer if Tapeti.Cmd exposed it's file format in a NuGet package... if only I knew the author ¯\_(ツ)_/¯
    public class SerializableMessage
    {
        //public ulong DeliveryTag;
        //public bool Redelivered;
        public string? Exchange;
        public string? RoutingKey;
        //public string? Queue;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local - must be settable by JSON deserialization
        public SerializableMessageProperties? Properties;

        public JObject? Body;
        public byte[]? RawBody;
    }


    public class SerializableMessageProperties
    {
        public string? AppId;
        //public string? ClusterId;
        public string? ContentEncoding;
        public string? ContentType;
        public string? CorrelationId;
        public byte? DeliveryMode;
        public string? Expiration;
        public IDictionary<string, string>? Headers;
        public string? MessageId;
        public byte? Priority;
        public string? ReplyTo;
        public long? Timestamp;
        public string? Type;
        public string? UserId;
    }
}
