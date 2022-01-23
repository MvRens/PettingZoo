using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PettingZoo.Core.ExportImport.Subscriber;

namespace PettingZoo.Tapeti.Export
{
    public abstract class BaseTapetiCmdExportImportFormat : IExportImportFormat
    {
        public string Filter => TapetiCmdImportExportStrings.TapetiCmdFilter;
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
