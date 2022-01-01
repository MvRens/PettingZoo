using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PettingZoo.UI.Tab
{
    public interface ITab : INotifyPropertyChanged
    {
        string Title { get; }
        ContentControl Content { get; }
    }


    public interface ITabToolbarCommands
    {
        IEnumerable<TabToolbarCommand> ToolbarCommands { get; }
    }


    public interface ITabActivate
    {
        void Activate();
        void Deactivate();
    }


    public interface ITabHostWindowNotify
    {
        void HostWindowChanged(Window? hostWindow);
    }


    public readonly struct TabToolbarCommand
    {
        public ICommand Command { get; }
        public string Caption { get; }
        public ImageSource Icon { get; }


        public TabToolbarCommand(ICommand command, string caption, ImageSource icon)
        {
            Command = command;
            Caption = caption;
            Icon = icon;
        }
    }
}
