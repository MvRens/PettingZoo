using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PettingZoo.UI
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        protected virtual void RaiseOtherPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        protected bool SetField<T>(ref T field, T value, IEqualityComparer<T>? comparer = null, [CallerMemberName] string? propertyName = null,
            params string[]? otherPropertiesChanged)
        {
            if ((comparer ?? EqualityComparer<T>.Default).Equals(field, value))
                return false;

            field = value;
            RaisePropertyChanged(propertyName);

            if (otherPropertiesChanged == null)
                return true;
            
            foreach (var otherProperty in otherPropertiesChanged)
                RaisePropertyChanged(otherProperty);
            
            return true;
        }
    }
}