using System.ComponentModel;
using System.Windows.Input;

namespace CarSharing.ViewModels
{
    internal class ViewModelBase : Notify, IDisposable, INotifyPropertyChanged
    {
        protected static readonly ICommand DisabledCommand = new CommandBase(() => { }, () => false);

        public event EventHandler? Disposed;

        public ViewModelBase() : base()
        {
        }

        protected override void DisposeConcrete()
        {
            try
            {
                var delegates = Disposed?.GetInvocationList()?.Cast<EventHandler>();

                if (delegates?.Any() == true)
                {
                    var eventArgs = new EventArgs();

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

                RaisePropertyChanged(nameof(IsDisposed));
            }
            catch { }
            finally
            {
                Disposed = null;
                
                base.DisposeConcrete();
            }
        }
    }
}
