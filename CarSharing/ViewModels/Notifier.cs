using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CarSharing.ViewModels
{
    internal abstract class Notify : SafeDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        protected override void DisposeConcrete()
        {
            PropertyChanged = null;
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var delegates = PropertyChanged?.GetInvocationList()?.Cast<PropertyChangedEventHandler>();

            if (delegates?.Any() == true)
            {
                var eventArgs = new PropertyChangedEventArgs(propertyName);

                foreach (var @delegate in delegates)
                {
                    try
                    {
                        @delegate?.Invoke(this, eventArgs);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
