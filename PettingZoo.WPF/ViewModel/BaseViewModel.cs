using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PettingZoo.WPF.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private int commandsChangedDisabled;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        protected bool SetField<T>(ref T field, T value, IEqualityComparer<T>? comparer = null, [CallerMemberName] string? propertyName = null,
            DelegateCommand[]? delegateCommandsChanged = null,
            string[]? otherPropertiesChanged = null)
        {
            if ((comparer ?? EqualityComparer<T>.Default).Equals(field, value))
                return false;

            field = value;
            RaisePropertyChanged(propertyName);

            if (otherPropertiesChanged != null)
            {
                foreach (var otherProperty in otherPropertiesChanged)
                    RaisePropertyChanged(otherProperty);
            }

            // ReSharper disable once InvertIf
            if (delegateCommandsChanged != null)
            {
                foreach (var delegateCommand in delegateCommandsChanged)
                    delegateCommand.RaiseCanExecuteChanged();
            }

            return true;
        }


        protected void DisableCommandsChanged(Action updateFields, params DelegateCommand[] delegateCommandsChangedAfter)
        {
            commandsChangedDisabled++;
            try
            {
                updateFields();
            }
            finally
            {
                commandsChangedDisabled--;
                if (commandsChangedDisabled == 0)
                {
                    foreach (var delegateCommand in delegateCommandsChangedAfter)
                        delegateCommand.RaiseCanExecuteChanged();
                }
            }
        }
    }
}