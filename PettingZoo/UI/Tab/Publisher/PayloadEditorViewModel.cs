using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows;
using ICSharpCode.AvalonEdit.Highlighting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PettingZoo.Core.Validation;
using PettingZoo.WPF.ViewModel;

namespace PettingZoo.UI.Tab.Publisher
{
    public enum PayloadEditorContentType
    {
        Json,
        Plain,
        Other
    }


    public enum ValidationStatus
    {
        NotSupported,
        Validating,
        Ok,
        OkSyntax,
        Error
    }


    public readonly struct ValidationInfo
    {
        public ValidationStatus Status { get; }
        public string Message { get; }
        public TextPosition? ErrorPosition { get; }


        public ValidationInfo(ValidationStatus status, string? message = null, TextPosition? errorPosition = null)
        {
            Status = status;
            Message = message ?? status switch
            {
                ValidationStatus.NotSupported => "",
                ValidationStatus.Validating => PayloadEditorStrings.ValidationValidating,
                ValidationStatus.Ok => PayloadEditorStrings.ValidationOk,
                ValidationStatus.OkSyntax => PayloadEditorStrings.ValidationOkSyntax,
                ValidationStatus.Error => throw new InvalidOperationException(@"Message required for Error validation status"),
                _ => throw new ArgumentException(@"Unsupported validation status", nameof(status))
            };
            ErrorPosition = errorPosition;
        }
    }


    public class PayloadEditorViewModel : BaseViewModel
    {
        private const string ContentTypeJson = "application/json";
        private const string ContentTypePlain = "text/plain";

        private string contentType = ContentTypeJson;
        private PayloadEditorContentType contentTypeSelection = PayloadEditorContentType.Json;
        private bool fixedJson;

        private ValidationInfo validationInfo = new(ValidationStatus.OkSyntax);

        private string payload = "";


        public string ContentType
        {
            get => ContentTypeSelection switch
            {
                PayloadEditorContentType.Json => ContentTypeJson,
                PayloadEditorContentType.Plain => ContentTypePlain,
                _ => contentType
            };

            set
            {
                if (!SetField(ref contentType, value))
                    return;

                ContentTypeSelection = value switch
                {
                    ContentTypeJson => PayloadEditorContentType.Json,
                    ContentTypePlain => PayloadEditorContentType.Plain,
                    _ => PayloadEditorContentType.Other
                };
            }
        }


        public PayloadEditorContentType ContentTypeSelection
        {
            get => contentTypeSelection;
            set
            {
                if (!SetField(ref contentTypeSelection, value, otherPropertiesChanged: new [] { nameof(ValidationVisibility), nameof(SyntaxHighlighting) }))
                    return;

                ContentType = ContentTypeSelection switch
                {
                    PayloadEditorContentType.Json => ContentTypeJson,
                    PayloadEditorContentType.Plain => ContentTypePlain,
                    _ => ContentType
                };

                ValidatePayload();
            }
        }


        public bool FixedJson
        {
            get => fixedJson;
            set => SetField(ref fixedJson, value);
        }


        public ValidationInfo ValidationInfo
        {
            get => validationInfo;
            private set => SetField(ref validationInfo, value, otherPropertiesChanged: new[] { nameof(ValidationOk), nameof(ValidationError), nameof(ValidationValidating), nameof(ValidationMessage) });
        }


        public Visibility ValidationVisibility => ContentTypeSelection == PayloadEditorContentType.Json ? Visibility.Visible : Visibility.Collapsed;

        public string ValidationMessage => ValidationInfo.Message;
        
        public Visibility ValidationOk => ValidationInfo.Status is ValidationStatus.Ok or ValidationStatus.OkSyntax ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ValidationError => ValidationInfo.Status == ValidationStatus.Error ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ValidationValidating => ValidationInfo.Status == ValidationStatus.Validating ? Visibility.Visible : Visibility.Collapsed;


        public Visibility ContentTypeVisibility => FixedJson ? Visibility.Collapsed : Visibility.Visible;


        public string Payload
        {
            get => payload;
            set => SetField(ref payload, value);
        }


        public IHighlightingDefinition? SyntaxHighlighting => ContentTypeSelection == PayloadEditorContentType.Json
            ? HighlightingManager.Instance.GetDefinition(@"Json")
            : null;


        public IPayloadValidator? Validator { get; set; }


        public PayloadEditorViewModel()
        {
            var observable = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => PropertyChanged += h,
                h => PropertyChanged -= h)
                .Where(e => e.EventArgs.PropertyName == nameof(Payload));

            observable
                .Subscribe(_ => ValidatingPayload());

            observable
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(_ => ValidatePayload());
        }


        private void ValidatingPayload()
        {
            if (ValidationInfo.Status == ValidationStatus.Validating)
                return;

            if (ContentTypeSelection != PayloadEditorContentType.Json)
            {
                ValidationInfo = new ValidationInfo(ValidationStatus.NotSupported);
                return;
            }

            ValidationInfo = new ValidationInfo(ValidationStatus.Validating);
        }


        private void ValidatePayload()
        {
            if (ContentTypeSelection != PayloadEditorContentType.Json)
            {
                ValidationInfo = new ValidationInfo(ValidationStatus.NotSupported);
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(Payload))
                {
                    if (Validator != null && Validator.CanValidate())
                    {
                        Validator.Validate(payload);
                        ValidationInfo = new ValidationInfo(ValidationStatus.Ok);
                    }
                    else
                    {
                        JToken.Parse(Payload);
                        ValidationInfo = new ValidationInfo(ValidationStatus.OkSyntax);
                    }
                }
                else
                    ValidationInfo = new ValidationInfo(ValidationStatus.OkSyntax);
            }
            catch (PayloadValidationException e)
            {
                ValidationInfo = new ValidationInfo(ValidationStatus.Error, e.Message, e.ErrorPosition);
            }
            catch (JsonSerializationException e)
            {
                ValidationInfo = new ValidationInfo(ValidationStatus.Error, e.Message, new TextPosition(e.LineNumber, e.LinePosition));
            }
            catch (JsonReaderException e)
            {
                ValidationInfo = new ValidationInfo(ValidationStatus.Error, e.Message, new TextPosition(e.LineNumber, e.LinePosition));
            }
            catch (Exception e)
            {
                ValidationInfo = new ValidationInfo(ValidationStatus.Error, e.Message);
            }
        }
    }


    public class DesignTimePayloadEditorViewModel : PayloadEditorViewModel
    {
    }
}
