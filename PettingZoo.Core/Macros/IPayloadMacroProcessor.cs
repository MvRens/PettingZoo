using System.Collections.Generic;

namespace PettingZoo.Core.Macros
{
    public interface IPayloadMacroProcessor
    {
        string Apply(string payload);

        IEnumerable<IPayloadMacro> Macros { get; }
    }


    public interface IPayloadMacro
    {
        public string DisplayName { get; }
        public string MacroText { get; }
    }
}
