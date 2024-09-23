using CarSharing.Abstractions;
using CarSharing.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Timer = System.Timers.Timer;

namespace CarSharing.ViewModels
{
    internal class DbViewModel : ViewModelBase
    {
        // грязный лайфхак чтобы не кешировать в dbContext не до конца сформированные данные
        private const int _dbCacheTimeoutSeconds = -2; 

        private readonly Timer? _timer;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private InvokeUIUpdate? _uiUpdateDelegate;
        private DbDataContext? _dbContext;
        private ILogger<DbViewModel>? _logger;
        private DateTime _lastDate;

        public int RefreshIntervalSeconds => (int)Math.Round((_timer?.Interval ?? 0) / 1000);
        
        public ICommandBase ChangeRefreshTimeoutCommand { get; }

        public ObservableCollection<CarDriverModel> Items { get; }

        public DbViewModel(DbDataContext dbContext, ILogger<DbViewModel> logger)
        {
            ArgumentNullException.ThrowIfNull(dbContext);
            ArgumentNullException.ThrowIfNull(logger);

            try
            {
                _dbContext = dbContext;
                _logger = logger;

                _cancellationTokenSource = new CancellationTokenSource();
                var currentDate = DateTime.Now.AddSeconds(_dbCacheTimeoutSeconds);

                Items = new(_dbContext.CarDriversView.Where(entity => entity.Date < currentDate).AsNoTracking().ToArray());

                _lastDate = Items.FirstOrDefault()?.Date ?? DateTime.MinValue;

                _timer = new(1000)
                {
                    AutoReset = false,
                };

                _timer.Elapsed += DbViewModel_DataRefresh;

                ChangeRefreshTimeoutCommand = new CommandBase<int>(
                    (parameter) =>
                    {
                        _timer.Interval = parameter * 1000;
                        _uiUpdateDelegate?.Invoke(() => RaisePropertyChanged(nameof(RefreshIntervalSeconds)));
                    },
                    (parameter) => parameter > 0 && parameter <= int.MaxValue / 1000);

                _timer.Start();
            }
            catch
            {
                _dbContext = null;
                _logger = null;
                
                using (_cancellationTokenSource)
                {
                    _cancellationTokenSource?.Cancel();
                }

                (ChangeRefreshTimeoutCommand as CommandBase<int>)?.Dispose();
                    
                if (_timer is not null)
                {
                    using (_timer)
                    {
                        _timer.Elapsed -= DbViewModel_DataRefresh;
                    }

                    _timer = null;
                }

                throw;
            }
        }

        private async void DbViewModel_DataRefresh(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (IsDisposed) return;

            _logger?.LogMethodCall(caller: this);

            try
            {
                
                var token = _cancellationTokenSource.Token;
               
                var currentDate = DateTime.Now.AddSeconds(_dbCacheTimeoutSeconds);
                if (currentDate <= _lastDate) return;

                var newItems = await _dbContext!.CarDriversView.Where(entity => entity.Date > _lastDate && entity.Date < currentDate).AsNoTracking().ToArrayAsync(token).ConfigureAwait(false);

                if (newItems.Length > 0)
                {
                    _uiUpdateDelegate?.Invoke(() =>
                    {
                        foreach (var item in newItems.Reverse())
                        {
                            if (token.IsCancellationRequested) return;

                            Items.Insert(0, item);
                            _lastDate = item.Date;
                        }
                    });
                }
            }
            catch (OperationCanceledException) { return; }
            catch (ObjectDisposedException) { return; }
            catch (Exception error) when (_logger?.FilterAndLogError(error) == true)
            {
            }

            if (!IsDisposed) _timer?.Start();
        }

        public void SetUiUpdateDelegate(InvokeUIUpdate? uiUpdateDelegate)
        {
            ThrowIfDisposed();

            _uiUpdateDelegate = uiUpdateDelegate;
        }

        protected override void DisposeConcrete()
        {
            _logger?.LogMethodCall(caller: this);

            try
            {
                using (_cancellationTokenSource)
                using (_timer)
                {
                    _timer!.Elapsed -= DbViewModel_DataRefresh;

                    _cancellationTokenSource.Cancel();
                    _timer!.Stop();
                }

                Items.Clear();
            }
            catch (Exception e) when (_logger?.FilterAndLogError(LogLevel.Critical, e) == true)
            {
            }
            finally
            {
                _dbContext = null;
                _logger = null;
                _uiUpdateDelegate = null;
            }

            base.DisposeConcrete();
        }
    }
}
