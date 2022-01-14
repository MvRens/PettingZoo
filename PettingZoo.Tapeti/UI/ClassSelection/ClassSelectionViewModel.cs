using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using PettingZoo.Core.Generator;
using PettingZoo.WPF.ViewModel;

namespace PettingZoo.Tapeti.UI.ClassSelection
{
    public class ClassSelectionViewModel : BaseViewModel
    {
        private BaseClassTreeItem? selectedItem;
        private readonly DelegateCommand selectCommand;

        private string filterText = "";
        private bool filterMessageOnly = true;
        private ObservableCollection<BaseClassTreeItem> filteredExamples;

        public ObservableCollection<BaseClassTreeItem> Examples { get; } = new();

        public string FilterText
        {
            get => filterText;
            set => SetField(ref filterText, value);
        }


        public bool FilterMessageOnly
        {
            get => filterMessageOnly;
            set => SetField(ref filterMessageOnly, value);
        }

        public ObservableCollection<BaseClassTreeItem> FilteredExamples
        {
            get => filteredExamples;

            [MemberNotNull(nameof(filteredExamples))]
            set => SetField(ref filteredExamples!, value);
        }


        public BaseClassTreeItem? SelectedItem
        {
            get => selectedItem; 
            set
            {
                if (!SetField(ref selectedItem, value))
                    return;

                selectCommand.RaiseCanExecuteChanged();
            }
        }


        public ICommand SelectCommand => selectCommand;


        public event EventHandler<IExample>? Select;


        public ClassSelectionViewModel(IEnumerable<IClassTypeExample> examples)
        {
            selectCommand = new DelegateCommand(SelectExecute, SelectCanExecute);

            TreeFromExamples(examples);

            Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => PropertyChanged += h,
                h => PropertyChanged -= h)
                .Where(e => e.EventArgs.PropertyName is nameof(FilterText) or nameof(FilterMessageOnly))
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(SynchronizationContext.Current!)
                .Subscribe(_ => UpdateFilteredExamples());

            UpdateFilteredExamples();
        }


        [MemberNotNull(nameof(filteredExamples))]
        private void UpdateFilteredExamples()
        {
            if (!FilterMessageOnly && string.IsNullOrWhiteSpace(FilterText))
                FilteredExamples = Examples;

            FilteredExamples = new ObservableCollection<BaseClassTreeItem>(Examples
                .Select(i => i.Filter(FilterText, FilterMessageOnly))
                .Where(i => i != null)
                .Cast<BaseClassTreeItem>());
        }


        private void SelectExecute()
        {
            if (SelectedItem is not ExampleTreeItem exampleTreeItem)
                return;

            Select?.Invoke(this, exampleTreeItem.Example);
        }


        private bool SelectCanExecute()
        {
            return SelectedItem is ExampleTreeItem;
        }


        private void TreeFromExamples(IEnumerable<IClassTypeExample> examples)
        {
            var root = new NamespaceFolderClassTreeItem(string.Empty);

            foreach (var example in examples)
            {
                var folder = !string.IsNullOrEmpty(example.Namespace) 
                    ? CreateFolder(root, example.Namespace.Split('.'))
                    : root;

                folder.AddChild(new ExampleTreeItem(example));
            }
            

            // If the first levels only consist of one child folder, collapse them into one entry
            var collapsedRoot = root;
            while (collapsedRoot.Children.Count == 1 && collapsedRoot.Children.First() is NamespaceFolderClassTreeItem newRoot)
                collapsedRoot = newRoot.Collapse(collapsedRoot.Name);

            if (ReferenceEquals(collapsedRoot, root))
            {
                foreach (var rootItem in root.Children)
                    Examples.Add(rootItem);
            }
            else
                Examples.Add(collapsedRoot);
        }


        private static NamespaceFolderClassTreeItem CreateFolder(NamespaceFolderClassTreeItem root, IEnumerable<string> parts)
        {
            var parent = root;

            foreach (var part in parts)
            {
                if (parent.Children.FirstOrDefault(c => c is NamespaceFolderClassTreeItem && c.Name == part) is NamespaceFolderClassTreeItem child)
                {
                    parent = child;
                    continue;
                }

                child = new NamespaceFolderClassTreeItem(part);
                parent.AddChild(child);

                parent = child;
            }

            return parent;
        }
    }


    public abstract class BaseClassTreeItem
    {
        private readonly SortedSet<BaseClassTreeItem> children = new(new BaseClassTreeItemComparer());

        public string Name { get; protected set; }
        public IReadOnlyCollection<BaseClassTreeItem> Children => children;


        protected BaseClassTreeItem(string name)
        {
            Name = name;
        }


        public void AddChild(BaseClassTreeItem item)
        {
            children.Add(item);
        }


        public abstract BaseClassTreeItem? Filter(string filterText, bool messageOnly);
    }


    public class NamespaceFolderClassTreeItem : BaseClassTreeItem
    {
        public NamespaceFolderClassTreeItem(string name) : base(name)
        {
        }


        public NamespaceFolderClassTreeItem Collapse(string parentFolderName)
        {
            Name = string.IsNullOrEmpty(parentFolderName) ? Name : parentFolderName + "." + Name;
            return this;
        }


        public override BaseClassTreeItem? Filter(string filterText, bool messageOnly)
        {
            var childFilterText = filterText;

            // If the folder name matches, include everything in it (...that matches messageOnly)
            if (!string.IsNullOrWhiteSpace(filterText) && Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase))
                childFilterText = "";

            var filteredChildren = Children
                .Select(c => c.Filter(childFilterText, messageOnly))
                .Where(c => c != null)
                .Cast<BaseClassTreeItem>();

            var result = new NamespaceFolderClassTreeItem(Name);
            var hasChildren = false;

            foreach (var filteredChild in filteredChildren)
            {
                result.AddChild(filteredChild);
                hasChildren = true;
            }

            return hasChildren ? result : null;
        }
    }


    public class ExampleTreeItem : BaseClassTreeItem
    {
        public IClassTypeExample Example { get; }


        public ExampleTreeItem(IClassTypeExample example) : base(example.ClassName)
        {
            Example = example;
        }


        public override BaseClassTreeItem? Filter(string filterText, bool messageOnly)
        {
            // Assumes examples don't have child items, so no further filtering is required
            if (messageOnly && !Name.EndsWith(@"Message", StringComparison.CurrentCultureIgnoreCase))
                return null;

            return string.IsNullOrWhiteSpace(filterText) || Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase) 
                ? this 
                : null;
        }
    }


    public class BaseClassTreeItemComparer : IComparer<BaseClassTreeItem>
    {
        public int Compare(BaseClassTreeItem? x, BaseClassTreeItem? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (y == null) return 1;
            if (x == null) return -1;

            if (x.GetType() != y.GetType())
                return x is NamespaceFolderClassTreeItem ? -1 : 1;

            return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }
    }


    public class DesignTimeClassSelectionViewModel : ClassSelectionViewModel
    {
        public DesignTimeClassSelectionViewModel() 
            : base(new IClassTypeExample[]
            {
                new DesignTimeExample("Messaging.Test", "Messaging.Test", "TestMessage"),
                new DesignTimeExample("Messaging.Test", "Messaging.Test", "SomeRequestMessage"),
                new DesignTimeExample("Messaging.Test", "Messaging.Test.Model", "SomeViewModel")
            })
        {

        }


        private class DesignTimeExample : IClassTypeExample
        {
            public string AssemblyName { get; }
            public string? Namespace { get; }
            public string ClassName { get; }


            public DesignTimeExample(string assemblyName, string? ns, string className)
            {
                AssemblyName = assemblyName;
                Namespace = ns;
                ClassName = className;
            }


            public string Generate()
            {
                return "";
            }


            public bool TryGetPublishDestination(out string exchange, out string routingKey)
            {
                exchange = "";
                routingKey = "";
                return false;
            }
        }
    }
}
