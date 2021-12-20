using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace PettingZoo.UI.Tab.Publisher
{
    public enum PayloadEditorContentType
    {
        Json,
        Plain,
        Other
    };


    public class PayloadEditorViewModel : BaseViewModel
    {
        private const string ContentTypeJson = "application/json";
        private const string ContentTypePlain = "text/plain";

        private string contentType = ContentTypeJson;
        private PayloadEditorContentType contentTypeSelection = PayloadEditorContentType.Json;
        private bool fixedJson;

        private bool jsonValid = true;
        private string jsonValidationMessage;

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
                if (!SetField(ref contentTypeSelection, value, otherPropertiesChanged: new [] { nameof(JsonValidationVisibility) }))
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

        public Visibility JsonValidationVisibility => ContentTypeSelection == PayloadEditorContentType.Json ? Visibility.Visible : Visibility.Collapsed;
        public Visibility JsonValidationOk => JsonValid ? Visibility.Visible : Visibility.Collapsed;
        public Visibility JsonValidationError => !JsonValid ? Visibility.Visible : Visibility.Collapsed;


        public string JsonValidationMessage
        {
            get => jsonValidationMessage;
            private set => SetField(ref jsonValidationMessage, value);
        }


        public bool JsonValid
        {
            get => jsonValid;
            private set => SetField(ref jsonValid, value, otherPropertiesChanged: new[] { nameof(JsonValidationOk), nameof(JsonValidationError) });
        }

        public Visibility ContentTypeVisibility => FixedJson ? Visibility.Collapsed : Visibility.Visible;


        public string Payload
        {
            get => payload;
            set => SetField(ref payload, value);
        }



        public PayloadEditorViewModel()
        {
            jsonValidationMessage = PayloadEditorStrings.JsonValidationOk;

            Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => PropertyChanged += h,
                h => PropertyChanged -= h)
                .Where(e => e.EventArgs.PropertyName == nameof(Payload))
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(_ => ValidatePayload());
        }


        private void ValidatePayload()
        {
            if (ContentTypeSelection != PayloadEditorContentType.Json)
            {
                JsonValid = true;
                JsonValidationMessage = PayloadEditorStrings.JsonValidationOk;
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(Payload))
                    JToken.Parse(Payload);

                JsonValid = true;
                JsonValidationMessage = PayloadEditorStrings.JsonValidationOk;
            }
            catch (Exception e)
            {
                JsonValid = false;
                JsonValidationMessage = string.Format(PayloadEditorStrings.JsonValidationError, e.Message);
            }
        }
    }


    public class DesignTimePayloadEditorViewModel : PayloadEditorViewModel
    {
    }
}
