using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PettingZoo.WPF.ViewModel;

namespace PettingZoo.UI.Tab.Undocked
{
    public class UndockedTabHostViewModel : BaseViewModel, ITabActivate
    {
        private readonly ITabHostProvider tabHostProvider;
        private readonly ITab tab;
        private readonly DelegateCommand dockCommand;
        private bool docked;


        public string Title => tab.Title;
        public ContentControl Content => tab.Content;
        public IEnumerable<TabToolbarCommand> ToolbarCommands => (tab as ITabToolbarCommands)?.ToolbarCommands ?? Enumerable.Empty<TabToolbarCommand>();

        public Visibility ToolbarCommandsSeparatorVisibility =>
            ToolbarCommands.Any() ? Visibility.Visible : Visibility.Collapsed;

        public ICommand DockCommand => dockCommand;


        public UndockedTabHostViewModel(ITabHostProvider tabHostProvider, ITab tab)
        {
            this.tabHostProvider = tabHostProvider;
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
            docked = true;
            tabHostProvider.Instance.DockTab(tab);
        }


        public void WindowClosed()
        {
            if (docked)
                return;

            tabHostProvider.Instance.UndockedTabClosed(tab);
            (tab as IDisposable)?.Dispose();
        }


        public void Activate()
        {
            (tab as ITabActivate)?.Activate();
        }

        public void Deactivate()
        {
            (tab as ITabActivate)?.Deactivate();
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