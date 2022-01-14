using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;

namespace PettingZoo.UI.Tab
{
    public class ViewTab<TView, TViewModel> : IDisposable, ITab, ITabToolbarCommands, ITabActivate, ITabHostWindowNotify where TView : ContentControl where TViewModel : INotifyPropertyChanged
    {
        public string Title => getTitle(viewModel);
        public ContentControl Content { get; }

        public IEnumerable<TabToolbarCommand> ToolbarCommands => viewModel is ITabToolbarCommands tabToolbarCommands 
            ? tabToolbarCommands.ToolbarCommands 
            : Enumerable.Empty<TabToolbarCommand>();

        public event PropertyChangedEventHandler? PropertyChanged;


        private readonly TViewModel viewModel;
        private readonly Func<TViewModel, string> getTitle;

        
        public ViewTab(TView view, TViewModel viewModel, Expression<Func<TViewModel, string>> title)
        {
            if (title.Body is not MemberExpression titleMemberExpression)
                throw new ArgumentException(@"Invalid expression type, expected viewModel => viewModel.TitlePropertyName", nameof(title));

            var titlePropertyName = titleMemberExpression.Member.Name;

            this.viewModel = viewModel;
            getTitle = title.Compile();
            Content = view;


            viewModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == titlePropertyName)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                
                else if (args.PropertyName == nameof(ToolbarCommands))
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolbarCommands)));
            };
        }


        public void Activate()
        {
            (viewModel as ITabActivate)?.Activate();
        }


        public void Deactivate()
        {
            (viewModel as ITabActivate)?.Deactivate();
        }


        public void HostWindowChanged(Window? hostWindow)
        {
            (viewModel as ITabHostWindowNotify)?.HostWindowChanged(hostWindow);
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
            (viewModel as IDisposable)?.Dispose();
        }
    }
}
