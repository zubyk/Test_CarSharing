using CarSharing.Abstractions;
using CarSharing.ViewModels;
using CarSharing.Windows;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace CarSharing
{
    public partial class MainWindow : Window
    {
        internal MainViewModel? ViewModel
        {
            get { return (MainViewModel)GetValue(ViewModelProperty); }
            private set { SetValue(ViewModelProperty, value); }
        }

        private static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(null));
        
        private ILogger<MainWindow>? _logger;
        private IWindowsCreator? _windowsCreator;

        internal MainWindow()
        {
            InitializeComponent();
        }

        internal MainWindow(MainViewModel viewModel, IWindowsCreator windowsCreator, ILogger<MainWindow> logger)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(windowsCreator);

            logger.LogMethodCall(caller: this);

            try
            {
                _logger = logger;
                _windowsCreator = windowsCreator;

                viewModel.SetUiUpdateDelegate(Dispatcher.Invoke);

                ViewModel = viewModel;

                InitializeComponent();
            }
            catch (Exception e) when (_logger!.FilterAndLogError(e))
            {
                _logger = null;
                _windowsCreator = null;

                viewModel.SetUiUpdateDelegate(null);
                
                ViewModel = null;

                throw;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _logger?.LogMethodCall(caller: this);
            
            try
            {
                ViewModel?.SetUiUpdateDelegate(null);
                ViewModel = null;
            }
            catch (Exception error) when (_logger?.FilterAndLogError(error) == true)
            {
            }
            finally
            {
                _logger = null;
                _windowsCreator = null;
            }

            base.OnClosed(e);
        }

        private void OpenDbFormButton_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogMethodCall(caller: this);

            var window = _windowsCreator?.GetWindow<DbViewWindow>();

            if (window is not null)
            {
                window.Owner = this;
                window.Show();
            }
        }
    }
}