using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PettingZoo.Core.Macros
{
    public class PayloadMacroProcessor : IPayloadMacroProcessor
    {
        private readonly BasePayloadMacro[] macros;
        public IEnumerable<IPayloadMacro> Macros => macros;


        public PayloadMacroProcessor()
        {
            macros = new BasePayloadMacro[]
            {
                new NewGuidPayloadMacro(),
                new JsonDateTimePayloadMacro()
            };
        }


        // For now we only support simple one-keyboard macros, but this could be extended with parameters if required
        private static readonly Regex MacroRegex = new("{{(.+?)}}", RegexOptions.Compiled);


        public string Apply(string payload)
        {
            return MacroRegex.Replace(payload, match =>
            {
                var macroCommand = match.Groups[1].Value.Trim();
                var macro = macros.FirstOrDefault(m => string.Equals(m.MacroCommand, macroCommand, StringComparison.CurrentCultureIgnoreCase));

                return macro != null 
                    ? macro.GetValue()
                    : match.Groups[0].Value;
            });
        }
    }
}
