using System;

namespace PettingZoo.Core.Macros
{
    public class JsonDateTimePayloadMacro : BasePayloadMacro
    {
        public JsonDateTimePayloadMacro()
            : base("JsonUtcNow", "Current date/time (yyyy-mm-ddThh:mm:ss.mmmZ)")
        {
        }


        public override string GetValue()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
        }
    }
}
