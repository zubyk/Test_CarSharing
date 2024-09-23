using System.ComponentModel;
using System.Windows.Input;

namespace CarSharing.Abstractions
{
    public interface ICommandBase : ICommand, INotifyPropertyChanged
    {
        bool IsExecuting { get; }
        void FireCanExecuteChanged();
    }
}