using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace PettingZoo.UI.Tab
{
    public class ViewTab<TView, TViewModel> : ITab where TView : ContentControl where TViewModel : INotifyPropertyChanged
    {
        public string Title => getTitle(viewModel);
        public ContentControl Content { get; }
        public ICommand CloseTabCommand { get; }

        public IEnumerable<TabToolbarCommand> ToolbarCommands => viewModel is ITabToolbarCommands tabToolbarCommands 
            ? tabToolbarCommands.ToolbarCommands 
            : Enumerable.Empty<TabToolbarCommand>();

        public event PropertyChangedEventHandler? PropertyChanged;


        private readonly TViewModel viewModel;
        private readonly Func<TViewModel, string> getTitle;

        
        public ViewTab(ICommand closeTabCommand, TView view, TViewModel viewModel, Expression<Func<TViewModel, string>> title)
        {
            if (title.Body is not MemberExpression titleMemberExpression)
                throw new ArgumentException(@"Invalid expression type, expected viewModel => viewModel.TitlePropertyName", nameof(title));

            var titlePropertyName = titleMemberExpression.Member.Name;

            CloseTabCommand = closeTabCommand;
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
    }
}
