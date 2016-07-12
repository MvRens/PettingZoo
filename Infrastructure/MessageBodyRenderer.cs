using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PettingZoo.Infrastructure
{
    public class MessageBodyRenderer
    {
        public static Dictionary<string, Func<byte[], string>> ContentTypeHandlers = new Dictionary<string, Func<byte[], string>>
        {
            { "application/json", RenderJson }
        };


        public static string Render(byte[] body, string contentType = "")
        {
            Func<byte[], string> handler;

            if (ContentTypeHandlers.TryGetValue(contentType, out handler))
                return handler(body);

            // ToDo hex output if required
            return Encoding.UTF8.GetString(body);
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
