using System;

namespace PettingZoo.Core.Validation
{
    public class PayloadValidationException : Exception
    {
        public TextPosition? ErrorPosition { get; }


        public PayloadValidationException(string message, TextPosition? errorPosition) : base(message)
        {
            ErrorPosition = errorPosition;
        }
    }



    public interface IPayloadValidator
    {
        bool CanValidate();

        /// <exception cref="PayloadValidationException" />
        void Validate(string payload);
    }
}
