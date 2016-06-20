using System;
using System.Collections.Generic;
using System.Text;

namespace PettingZoo.Model
{
    public class MessageBodyRenderer
    {
        public static List<String> TextTypes = new List<string>
        {
            "application/json",
            "application/xml"
        };


        public static string Render(byte[] body, string contentType = "")
        {
            if (TextTypes.Contains(contentType))
            {
                return Encoding.UTF8.GetString(body);
            }

            // ToDo hex output
            return "";
        }
    }
}
