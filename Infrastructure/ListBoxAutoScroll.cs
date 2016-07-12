using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;

namespace PettingZoo.Infrastructure
{
    // Source: https://social.msdn.microsoft.com/Forums/vstudio/en-US/0f524459-b14e-4f9a-8264-267953418a2d/trivial-listboxlistview-autoscroll?forum=wpf
    //
    // Slightly modified version; takes into account the current scroll position of the ListBox,
    // so the end user can scroll up without getting dragged down again.
    public static class ListBox
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof (bool), typeof (System.Windows.Controls.ListBox),
                new PropertyMetadata(false));

        public static readonly DependencyProperty AutoScrollHandlerProperty =
            DependencyProperty.RegisterAttached("AutoScrollHandler", typeof (AutoScrollHandler),
                typeof (System.Windows.Controls.ListBox));

        public static bool GetAutoScroll(System.Windows.Controls.ListBox instance)
        {
            return (bool) instance.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(System.Windows.Controls.ListBox instance, bool value)
        {
            var oldHandler = (AutoScrollHandler) instance.GetValue(AutoScrollHandlerProperty);
            if (oldHandler != null)
            {
                oldHandler.Dispose();
                instance.SetValue(AutoScrollHandlerProperty, null);
            }

            instance.SetValue(AutoScrollProperty, value);
            if (value)
                instance.SetValue(AutoScrollHandlerProperty, new AutoScrollHandler(instance));
        }
    }


    public class AutoScrollHandler : DependencyObject, IDisposable
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof (IEnumerable),
                typeof (AutoScrollHandler), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None,
                    ItemsSourcePropertyChanged));

        private readonly System.Windows.Controls.ListBox target;
        private ScrollViewer scrollViewer;

        public AutoScrollHandler(System.Windows.Controls.ListBox target)
        {
            this.target = target;

            var binding = new Binding("ItemsSource")
            {
                Source = this.target
            };

            BindingOperations.SetBinding(this, ItemsSourceProperty, binding);
        }


        public void Dispose()
        {
            BindingOperations.ClearBinding(this, ItemsSourceProperty);
        }


        public IEnumerable ItemsSource
        {
            get { return (IEnumerable) GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }


        private static void ItemsSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((AutoScrollHandler) o).ItemsSourceChanged((IEnumerable) e.OldValue, (IEnumerable) e.NewValue);
        }


        private void ItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            var collection = oldValue as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged -= Collection_CollectionChanged;

            collection = newValue as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += Collection_CollectionChanged;
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems == null || e.NewItems.Count < 1)
                return;

            // Resolve on demand; in the constructor FindScrollViewer will return null
            if (scrollViewer == null)
            {
                scrollViewer = FindScrollViewer(target);
                if (scrollViewer == null)
                    return;
            }

            if (Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 1)
                target.ScrollIntoView(e.NewItems[e.NewItems.Count - 1]);
        }


        private static ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            var childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var childIndex = 0; childIndex < childCount; childIndex++)
            {
                var child = VisualTreeHelper.GetChild(parent, childIndex);
                var scrollViewer = (child as ScrollViewer);
                if (scrollViewer != null) 
                    return scrollViewer;

                scrollViewer = FindScrollViewer(child);
                if (scrollViewer != null) 
                    return scrollViewer;
            }

            return null;
        }
    }
}
