using System;

namespace PettingZoo.Core.Macros
{
    public class NewGuidPayloadMacro : BasePayloadMacro
    {
        public NewGuidPayloadMacro()
            : base("NewGuid", "Generate GUID")
        {
        }


        public override string GetValue()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
