using CarSharing.ViewModels;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace CarSharing.Windows
{
    public partial class DbViewWindow : Window
    {
        internal DbViewModel? ViewModel
        {
            get { return (DbViewModel)GetValue(ViewModelProperty); }
            private set { SetValue(ViewModelProperty, value); }
        }

        private static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(DbViewModel), typeof(DbViewWindow), new PropertyMetadata(null));

        private ILogger<DbViewWindow>? _logger;

        internal DbViewWindow()
        {
            InitializeComponent();
        }

        internal DbViewWindow(DbViewModel viewModel, ILogger<DbViewWindow> logger)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(logger);

            logger.LogMethodCall(caller: this);

            try
            {
                _logger = logger;

                viewModel.SetUiUpdateDelegate(Dispatcher.Invoke);

                ViewModel = viewModel;

                InitializeComponent();
            }
            catch (Exception e) when (_logger!.FilterAndLogError(e))
            {
                _logger = null;

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
            }

            base.OnClosed(e);
        }
    }
}
