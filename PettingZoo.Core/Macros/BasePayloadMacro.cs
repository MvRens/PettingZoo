namespace PettingZoo.Core.Macros
{
    public abstract class BasePayloadMacro : IPayloadMacro
    {
        public string DisplayName { get; }
        public string MacroText { get; }

        public string MacroCommand { get; }


        protected BasePayloadMacro(string macroCommand, string displayName)
        {
            MacroCommand = macroCommand;

            DisplayName = displayName;
            MacroText = "{{" + macroCommand + "}}";
        }


        public abstract string GetValue();
    }
}
