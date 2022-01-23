using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using PettingZoo.Core.Connection;
using PettingZoo.Core.ExportImport.Subscriber;
using PettingZoo.Core.Rendering;
using PettingZoo.WPF.ProgressWindow;
using PettingZoo.WPF.ViewModel;
using Serilog;
using Application = System.Windows.Application;
using IConnection = PettingZoo.Core.Connection.IConnection;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Threading.Timer;

namespace PettingZoo.UI.Tab.Subscriber
{
    public class SubscriberViewModel : BaseViewModel, IDisposable, ITabToolbarCommands, ITabActivate
    {
        private readonly ILogger logger;
        private readonly ITabFactory tabFactory;
        private readonly IConnection? connection;
        private readonly ISubscriber subscriber;
        private readonly IExportImportFormatProvider exportImportFormatProvider;
        private ReceivedMessageInfo? selectedMessage;
        private readonly DelegateCommand clearCommand;
        private readonly DelegateCommand exportCommand;
        private readonly TabToolbarCommand[] toolbarCommands;
        private IDictionary<string, string>? selectedMessageProperties;

        private readonly DelegateCommand createPublisherCommand;

        private bool tabActive;
        private Timer? newMessageTimer;
        private int unreadCount;


        public ICommand ClearCommand => clearCommand;
        public ICommand ExportCommand => exportCommand;

        // ReSharper disable once UnusedMember.Global - it is, but via a proxy
        public ICommand CreatePublisherCommand => createPublisherCommand;

        public ObservableCollectionEx<ReceivedMessageInfo> Messages { get; }
        public ObservableCollectionEx<ReceivedMessageInfo> UnreadMessages { get; }

        public ReceivedMessageInfo? SelectedMessage
        {
            get => selectedMessage;
            set
            {
                if (SetField(ref selectedMessage, value, otherPropertiesChanged: new[] { nameof(SelectedMessageBody) }))
                    UpdateSelectedMessageProperties();
            }
        }


        public Visibility UnreadMessagesVisibility => UnreadMessages.Count > 0 ? Visibility.Visible : Visibility.Collapsed;


        public string SelectedMessageBody =>
            SelectedMessage != null
                ? MessageBodyRenderer.Render(SelectedMessage.Body, SelectedMessage.Properties.ContentType)
                : "";

        public IDictionary<string, string>? SelectedMessageProperties
        {
            get => selectedMessageProperties;
            set => SetField(ref selectedMessageProperties, value);
        }

        public string Title
        {
            get
            {
                var title = new StringBuilder();

                if (IsReplyTab)
                    title.Append(SubscriberViewStrings.ReplyTabTitle);
                else if (subscriber.Exchange != null)
                    title.Append(subscriber.Exchange).Append(" - ").Append(subscriber.RoutingKey);
                else
                    title.Append(subscriber.QueueName);

                if (!tabActive && unreadCount > 0)
                    title.Append(" (").Append(unreadCount).Append(')');

                return title.ToString();
            }
        }


        public IEnumerable<TabToolbarCommand> ToolbarCommands => toolbarCommands;


        public bool IsReplyTab { get; }

        // ReSharper disable UnusedMember.Global - used via BindingProxy
        public Visibility StandardTabVisibility => !IsReplyTab ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ReplyTabVisibility => IsReplyTab ? Visibility.Visible : Visibility.Collapsed;
        // ReSharper restore UnusedMember.Global


        public SubscriberViewModel(ILogger logger, ITabFactory tabFactory, IConnection? connection, ISubscriber subscriber, IExportImportFormatProvider exportImportFormatProvider, bool isReplyTab)
        {
            IsReplyTab = isReplyTab;

            this.logger = logger;
            this.tabFactory = tabFactory;
            this.connection = connection;
            this.subscriber = subscriber;
            this.exportImportFormatProvider = exportImportFormatProvider;

            Messages = new ObservableCollectionEx<ReceivedMessageInfo>();
            Messages.AddRange(subscriber.GetInitialMessages());

            UnreadMessages = new ObservableCollectionEx<ReceivedMessageInfo>();

            clearCommand = new DelegateCommand(ClearExecute, HasMessagesCanExecute);
            exportCommand = new DelegateCommand(ExportExecute, HasMessagesCanExecute);

            toolbarCommands = new[]
            {
                new TabToolbarCommand(ClearCommand, SubscriberViewStrings.CommandClear, SvgIconHelper.LoadFromResource("/Images/Clear.svg")),
                new TabToolbarCommand(ExportCommand, SubscriberViewStrings.CommandExport, SvgIconHelper.LoadFromResource("/Images/Export.svg"))
            };

            createPublisherCommand = new DelegateCommand(CreatePublisherExecute, CreatePublisherCanExecute);

            subscriber.MessageReceived += SubscriberMessageReceived;
            subscriber.Start();
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
            newMessageTimer?.Dispose();
            subscriber.Dispose();
        }


        private void ClearExecute()
        {
            Messages.Clear();
            UnreadMessages.Clear();

            HasMessagesChanged();
            RaisePropertyChanged(nameof(UnreadMessagesVisibility));
        }


        private bool HasMessagesCanExecute()
        {
            return Messages.Count > 0 || UnreadMessages.Count > 0;
        }


        private void HasMessagesChanged()
        {
            clearCommand.RaiseCanExecuteChanged();
            exportCommand.RaiseCanExecuteChanged();
        }


        private void ExportExecute()
        {
            var formats = exportImportFormatProvider.ExportFormats.ToArray();

            var dialog = new SaveFileDialog
            {
                Filter = string.Join('|', formats.Select(f => f.Filter))
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            // 1-based? Seriously?
            if (dialog.FilterIndex <= 0 || dialog.FilterIndex > formats.Length)
                return;

            var messages = Messages.Concat(UnreadMessages).ToArray();
            var filename = dialog.FileName;
            var format = formats[dialog.FilterIndex - 1];

            var progressWindow = new ProgressWindow(SubscriberViewStrings.ExportProgressWindowTitle)
            {
                Owner = Application.Current.MainWindow,  // TODO I forgot if we have access to our host window here (which can be main or undocked), would be nicer if we did
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            progressWindow.Show();


            Task.Run(async () =>
            {
                try
                {
                    await using (var exportFile = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        await format.Export(
                            exportFile, 
                            new ListEnumerableProgressDecorator<ReceivedMessageInfo>(messages, progressWindow), 
                            progressWindow.CancellationToken);
                    }

                    if (progressWindow.CancellationToken.IsCancellationRequested)
                        return;

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        progressWindow.Close();
                        progressWindow = null;

                        MessageBox.Show(string.Format(SubscriberViewStrings.ExportSuccess, messages.Length, filename),
                            SubscriberViewStrings.ExportResultTitle,
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }
                catch (OperationCanceledException)
                {
                    // User cancelled
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error while exporting messages");

                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        progressWindow?.Close();
                        progressWindow = null;

                        MessageBox.Show(string.Format(SubscriberViewStrings.ExportError, e.Message),
                            SubscriberViewStrings.ExportResultTitle,
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }
                finally
                {
                    if (progressWindow != null)
                        await Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            progressWindow.Close();
                        });
                }
            });
        }


        private void CreatePublisherExecute()
        {
            if (connection == null)
                return;

            tabFactory.CreatePublisherTab(connection, SelectedMessage);
        }


        private bool CreatePublisherCanExecute()
        {
            return connection != null && SelectedMessage != null;
        }


        private void SubscriberMessageReceived(object? sender, MessageReceivedEventArgs args)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                if (!tabActive)
                {
                    unreadCount++;
                    RaisePropertyChanged(nameof(Title));

                    UnreadMessages.Add(args.MessageInfo);
                    if (UnreadMessages.Count == 1)
                        RaisePropertyChanged(nameof(UnreadMessagesVisibility));
                }
                else
                    Messages.Add(args.MessageInfo);

                HasMessagesChanged();
            });
        }


        private void UpdateSelectedMessageProperties()
        {
            createPublisherCommand.RaiseCanExecuteChanged();

            SelectedMessageProperties = SelectedMessage != null
                ? MessagePropertiesRenderer.Render(SelectedMessage.Properties)
                : null;
        }


        public void Activate()
        {
            tabActive = true;
            unreadCount = 0;

            RaisePropertyChanged(nameof(Title));

            if (UnreadMessages.Count == 0) 
                return;

            newMessageTimer?.Dispose();
            newMessageTimer = new Timer(
                _ =>
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (UnreadMessages.Count == 0)
                            return;

                        Messages.BeginUpdate();
                        UnreadMessages.BeginUpdate();
                        try
                        {
                            Messages.AddRange(UnreadMessages);
                            UnreadMessages.Clear();
                        }
                        finally
                        {
                            UnreadMessages.EndUpdate();
                            Messages.EndUpdate();
                        }

                        RaisePropertyChanged(nameof(UnreadMessagesVisibility));
                    });
                },
                null,
                TimeSpan.FromSeconds(5),
                Timeout.InfiniteTimeSpan);
        }


        public void Deactivate()
        {
            if (newMessageTimer != null)
            {
                newMessageTimer.Dispose();
                newMessageTimer = null;
            }

            tabActive = false;
        }
    }
    
    
    public class DesignTimeSubscriberViewModel : SubscriberViewModel
    {
        public DesignTimeSubscriberViewModel() : base(null!, null!, null!, new DesignTimeSubscriber(), null!, false)
        {
            for (var i = 1; i <= 5; i++)
                (i > 2 ? UnreadMessages : Messages).Add(new ReceivedMessageInfo(
                    "designtime",
                    $"designtime.message.{i}",
                    Encoding.UTF8.GetBytes(@"Design-time message"),
                    new MessageProperties(null)
                    {
                        ContentType = "text/fake",
                        ReplyTo = "/dev/null"
                    },
                    DateTime.Now));

            SelectedMessage = UnreadMessages[0];
        }


        private class DesignTimeSubscriber : ISubscriber
        {
            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }


            public string QueueName => "dummy";
            public string Exchange => "dummy";
            public string RoutingKey => "dummy";

            #pragma warning disable CS0067
            public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
            #pragma warning restore CS0067


            public IEnumerable<ReceivedMessageInfo> GetInitialMessages()
            {
                return Enumerable.Empty<ReceivedMessageInfo>();
            }


            public void Start()
            {
            }
        }
    }
}
