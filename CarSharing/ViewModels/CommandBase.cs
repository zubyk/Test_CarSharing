using CarSharing.Abstractions;
using System.Windows.Input;

namespace CarSharing.ViewModels
{
    internal class CommandBase<T>(Action<T> executeAction, Func<T, bool> canExecuteAction) : Notify, ICommand, ICommandBase
    {
        private readonly Type _type = typeof(T);
        private readonly Action<T> _executeAction = executeAction ?? ((parameter) => { });
        private readonly Func<T, bool> _canExecuteAction = canExecuteAction ?? ((parameter) => true);

        private volatile bool _isExecuting = false;

        public bool IsExecuting
        {
            get
            {
                return _isExecuting;
            }
            protected set
            {
                if (value != _isExecuting)
                {
                    _isExecuting = value;
                    RaisePropertyChanged(nameof(IsExecuting));
                }
            }
        }

        public event EventHandler? CanExecuteChanged;

        public void FireCanExecuteChanged()
        {
            var delegates = CanExecuteChanged?.GetInvocationList()?.Cast<EventHandler>();

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
        }

        public void Execute(object? parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    FireCanExecuteChanged();

                    _executeAction((T)parameter);
                }
                finally
                {
                    _isExecuting = false;
                    FireCanExecuteChanged();
                }
            }
        }

        public bool CanExecute(object? parameter) => !_isExecuting && !IsDisposed && ValidateParameter(parameter) && _canExecuteAction((T)parameter);

        private bool ValidateParameter(object? parameter) => parameter is null ? !_type.IsValueType && !_type.IsEnum : _type.IsAssignableFrom(parameter.GetType());

        protected override void DisposeConcrete()
        {
            CanExecuteChanged = null;
            base.DisposeConcrete();
        }
    }

    internal class CommandBase(Action executeAction, Func<bool>? canExecuteAction = null) : Notify, ICommand, ICommandBase
    {
        private readonly Action _executeAction = executeAction ?? (() => { });
        private readonly Func<bool> _canExecuteAction = canExecuteAction ?? (() => true);

        private volatile bool _isExecuting = false;

        public bool IsExecuting
        {
            get
            {
                return _isExecuting;
            }
            protected set
            {
                if (value != _isExecuting)
                {
                    _isExecuting = value;
                    RaisePropertyChanged(nameof(IsExecuting));
                }
            }
        }

        public event EventHandler? CanExecuteChanged;

        public void FireCanExecuteChanged()
        {
            var delegates = CanExecuteChanged?.GetInvocationList()?.Cast<EventHandler>();

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
        }

        public void Execute(object? parameter) => Execute();

        public bool CanExecute(object? parameter) => CanExecute();

        public void Execute()
        {
            if (CanExecute())
            {
                try
                {
                    IsExecuting = true;
                    FireCanExecuteChanged();

                    _executeAction();
                }
                finally
                {
                    IsExecuting = false;
                    FireCanExecuteChanged();
                }
            }
        }

        public bool CanExecute() => !_isExecuting && !IsDisposed && _canExecuteAction();

        protected override void DisposeConcrete()
        {
            CanExecuteChanged = null;
            base.DisposeConcrete();
        }
    }
}
