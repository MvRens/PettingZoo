using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PettingZoo.Core.Rendering
{
    public class MessageBodyRenderer
    {
        private static readonly Dictionary<string, Func<byte[], string>> ContentTypeHandlers = new()
        {
            { "application/json", RenderJson }
        };


        public static string Render(byte[] body, string? contentType)
        {
            return contentType != null && ContentTypeHandlers.TryGetValue(contentType, out var handler)
                ? handler(body) 
                : Encoding.UTF8.GetString(body);
        }


        public static string RenderJson(byte[] body)
        {
            var bodyText = Encoding.UTF8.GetString(body);
            try
            {
                using var stringReader = new StringReader(bodyText);
                using var jsonTextReader = new JsonTextReader(stringReader);
                using var stringWriter = new StringWriter();
                using var jsonWriter = new JsonTextWriter(stringWriter);

                jsonWriter.Formatting = Formatting.Indented;

                while (jsonTextReader.Read())
                    jsonWriter.WriteToken(jsonTextReader);

                jsonWriter.Flush();
                return stringWriter.ToString();
            }
            catch
            {
                return bodyText;
            }
        }
    }
}
