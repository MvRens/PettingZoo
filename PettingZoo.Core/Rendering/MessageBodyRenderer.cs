using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PettingZoo.Core.Rendering
{
    public class MessageBodyRenderer
    {
        public static Dictionary<string, Func<byte[], string>> ContentTypeHandlers = new()
        {
            { "application/json", RenderJson }
        };


        public static string Render(byte[] body, string contentType = "")
        {
            return ContentTypeHandlers.TryGetValue(contentType, out var handler)
                ? handler(body) 
                : Encoding.UTF8.GetString(body);

            // ToDo hex output if required
        }


        public static string RenderJson(byte[] body)
        {
            var bodyText = Encoding.UTF8.GetString(body);
            try
            {
                var obj = JsonConvert.DeserializeObject(bodyText);
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            catch
            {
                return bodyText;
            }
        }
    }
}
