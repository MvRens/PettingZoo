using System;
using System.Threading.Tasks;
using System.Windows;
using PettingZoo.Core.ExportImport.Publisher;
using PettingZoo.Core.Settings;
using PettingZoo.WPF.ViewModel;

namespace PettingZoo.UI.Tab.Publisher
{
    public class StoredPublisherMessagesViewModel
    {
        private readonly IPublisherMessagesRepository publisherMessagesRepository;


        public ObservableCollectionEx<StoredPublisherMessage> StoredMessages { get; } = new();


        public StoredPublisherMessagesViewModel(IPublisherMessagesRepository publisherMessagesRepository)
        {
            this.publisherMessagesRepository = publisherMessagesRepository;


            Task.Run(async () =>
            {
                var messages = await publisherMessagesRepository.GetStored();

                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    StoredMessages.ReplaceAll(messages);
                });
            });
        }


        public void Save(StoredPublisherMessage overwriteMessage, PublisherMessage message, Action<StoredPublisherMessage> onSaved)
        {
            Task.Run(async () =>
            {
                var updatedMessage = await publisherMessagesRepository.Update(overwriteMessage.Id, overwriteMessage.DisplayName, message);

                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    var index = StoredMessages.IndexOf(overwriteMessage);
                    if (index >= 0)
                        StoredMessages[index] = updatedMessage;
                    else
                        // Should not occur, but might as well handle it gracefully
                        StoredMessages.Add(updatedMessage);

                    onSaved(updatedMessage);
                });
            });
        }


        public void SaveAs(PublisherMessage message, string? originalDisplayName, Action<StoredPublisherMessage> onSaved)
        {
            var displayName = originalDisplayName ?? "";
            if (!InputDialog.Execute(ref displayName, StoredPublisherMessagesStrings.DisplayNameDialogTitle))
                return;

            Task.Run(async () =>
            {
                var storedMessage = await publisherMessagesRepository.Add(displayName, message);

                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    StoredMessages.Add(storedMessage);
                    onSaved(storedMessage);
                });
            });
        }


        public void Delete(StoredPublisherMessage message, Action onDeleted)
        {
            if (MessageBox.Show(
                    string.Format(StoredPublisherMessagesStrings.DeleteConfirmation, message.DisplayName),
                    StoredPublisherMessagesStrings.DeleteConfirmationTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Task.Run(async () =>
            {
                await publisherMessagesRepository.Delete(message.Id);

                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    StoredMessages.Remove(message);
                    onDeleted();
                });
            });
        }
    }
}
