using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PettingZoo.WPF.ViewModel;

namespace PettingZoo.UI.Tab.Undocked
{
    public class UndockedTabHostViewModel : BaseViewModel
    {
        private readonly ITabHost tabHost;
        private readonly ITab tab;
        private readonly DelegateCommand dockCommand;


        public string Title => tab.Title;
        public ContentControl Content => tab.Content;
        public IEnumerable<TabToolbarCommand> ToolbarCommands => (tab as ITabToolbarCommands)?.ToolbarCommands ?? Enumerable.Empty<TabToolbarCommand>();

        public Visibility ToolbarCommandsSeparatorVisibility =>
            ToolbarCommands.Any() ? Visibility.Visible : Visibility.Collapsed;

        public ICommand DockCommand => dockCommand;


        public UndockedTabHostViewModel(ITabHost tabHost, ITab tab)
        {
            this.tabHost = tabHost;
            this.tab = tab;

            tab.PropertyChanged += (_, args) =>
            {
                RaisePropertyChanged(args.PropertyName);
                if (args.PropertyName == nameof(ToolbarCommands))
                    RaisePropertyChanged(nameof(ToolbarCommandsSeparatorVisibility));
            };

            dockCommand = new DelegateCommand(DockCommandExecute);
        }


        private void DockCommandExecute()
        {
            tabHost.DockTab(tab);
        }


        public void WindowClosed()
        {
            tabHost.UndockedTabClosed(tab);
        }
    }


    public class DesignTimeUndockedTabHostViewModel : UndockedTabHostViewModel
    {
        public DesignTimeUndockedTabHostViewModel() : base(null!, new DesignTimeTab())
        {
        }


        private class DesignTimeTab : ITab
        {
            #pragma warning disable CS0067 // "The event ... is never used" - it's part of the interface so it's required.
            public event PropertyChangedEventHandler? PropertyChanged;
            #pragma warning restore CS0067

            public string Title => "Design-time tab title";
            public ContentControl Content => null!;
        }
    }
}