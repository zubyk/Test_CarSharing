using CarSharing.Abstractions;
using CarSharing.ViewModels;
using CarSharing.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace CarSharing
{
    internal class App : Application, IWindowsCreator
    {
        private readonly ILogger<App> _logger;
        private readonly IServiceScope _serviceScope;

        public App(IServiceProvider services, ILogger<App> logger)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(logger);

            logger.LogMethodCall(caller: this);
    
            _logger = logger;
            _serviceScope = services.CreateScope();

            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        public T GetWindow<T>() where T : Window
        {
            var requestedType = typeof(T);
            
            _logger.LogMethodCall(caller: this, args: requestedType);

            T? window = Windows.OfType<T>().FirstOrDefault();

            if (window is not null) return window;

            var services = _serviceScope.ServiceProvider;

            if (typeof(T) == typeof(MainWindow))
            {
                return (new MainWindow(
                    viewModel: services.GetService<MainViewModel>()!,
                    windowsCreator: this as IWindowsCreator,
                    logger: services.GetService<ILogger<MainWindow>>()!
                ) as T)!;
            }
            else if (typeof(T) == typeof(DbViewWindow))
            {
                return (new DbViewWindow(
                    viewModel: services.GetService<DbViewModel>()!,
                    logger: services.GetService<ILogger<DbViewWindow>>()!
                ) as T)!;
            }

            throw new NotSupportedException($"Creation of {requestedType} window not supported");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.LogMethodCall(caller: this);

            try
            {
                _serviceScope?.Dispose();
            }
            catch (Exception error) when (_logger.FilterAndLogError(error))
            {
            }

            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            GetWindow<MainWindow>().Show();

            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                
                _logger?.LogExecutionError(LogLevel.Critical, $"{nameof(DispatcherUnhandledException)} raised", e.Exception);

                MessageBox.Show(e.Exception.Message);
            }
        }
    }
}
