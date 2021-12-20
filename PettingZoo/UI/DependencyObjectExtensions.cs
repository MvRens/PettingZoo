using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace PettingZoo.UI
{
    public static class DependencyObjectExtensions
    {
        public static IObservable<T> OnPropertyChanges<T>(this DependencyObject source, DependencyProperty property)
        {
            return Observable.Create<T>(o => 
            {
                var dpd = DependencyPropertyDescriptor.FromProperty(property, property.OwnerType);
                if (dpd == null)
                    o.OnError(new InvalidOperationException("Can not register change handler for this dependency property."));

                void Handler(object? sender, EventArgs e)
                {
                    o.OnNext((T)source.GetValue(property));
                }

                dpd?.AddValueChanged(source, Handler);
                return Disposable.Create(() => dpd?.RemoveValueChanged(source, Handler));
            });
        }
    }
}
