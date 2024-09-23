using CarSharing.Abstractions;
using CarSharing.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace CarSharing.Services
{
    internal class DataGeneratorService<TConfiguration, TStorage> : SafeDisposable, IDataGenerator
        where TConfiguration : DataGeneratorConfiguration
        where TStorage : IDataStorage
    {
        private readonly ConcurrentQueue<DateTime> _datesQueue;
        private readonly ImmutableArray<string> _dataTemplate;
        private readonly int _generatorResolution;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private IDataStorage? _storage;
        private ITimeSignal? _timer;
        private ILogger<DataGeneratorService<TConfiguration, TStorage>>? _logger;

        private volatile bool _isRunning = false;
        private Task? _dataProcessingTask;
        
        public event EventHandler<GeneratedDataEventArgs>? DataGenerated;
        public bool IsRunning => _isRunning;

        public DataGeneratorService(TConfiguration configuration, ITimeSignal timer, TStorage storage, ILogger<DataGeneratorService<TConfiguration, TStorage>> logger)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(timer);
            ArgumentNullException.ThrowIfNull(storage);
            ArgumentNullException.ThrowIfNull(logger);

            if (configuration.GeneratorResolutionSeconds <= 0) throw new ArgumentException($"{nameof(configuration.GeneratorResolutionSeconds)} must be greater than zero", nameof(configuration));
            if (configuration.Data?.Any(s => !string.IsNullOrWhiteSpace(s)) != true) throw new ArgumentException($"{nameof(configuration.Data)} have no values", nameof(configuration));

            _dataTemplate = configuration.Data.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().Select(string.Intern).ToImmutableArray();

            _generatorResolution = configuration.GeneratorResolutionSeconds;

            try
            {
                _cancellationTokenSource = new();
                _datesQueue = new();

                _logger = logger;
                _storage = storage;
                _timer = timer;

                timer.Tick += Timer_Tick;
            }
            catch
            {
                using (_cancellationTokenSource)
                {
                    _storage = null;
                    _timer = null;
                    _logger = null;

                    timer.Tick -= Timer_Tick;
                }

                throw;
            }
        }

        private void Timer_Tick(object? sender, DateTime date)
        {
            if (_isRunning && date.Second % _generatorResolution == 0)
            {
                _datesQueue.Enqueue(date);
            }
        }

        public void Start()
        {
            ThrowIfDisposed();

            _logger?.LogMethodCall(caller: this);

            if (!_isRunning)
            {
                _isRunning = true;

                try
                {
                    if (_dataProcessingTask is null
                        || _dataProcessingTask.Status == TaskStatus.Canceled
                        || _dataProcessingTask.Status == TaskStatus.RanToCompletion
                        || _dataProcessingTask.Status == TaskStatus.Faulted)
                    {
                        var token = _cancellationTokenSource.Token;

                        _dataProcessingTask = GenerateAndStoreData(token);
                    }

                    if (_dataProcessingTask.Status == TaskStatus.Created) _dataProcessingTask.Start();
                }
                catch (Exception e) when (_logger?.FilterAndLogError(e) == true)
                {
                }
                finally
                {
                    _isRunning = _dataProcessingTask is not null && (
                        _dataProcessingTask?.Status != TaskStatus.Canceled 
                        || _dataProcessingTask?.Status != TaskStatus.Canceled 
                        || _dataProcessingTask?.Status != TaskStatus.Faulted);
                }
            }
        }

        public void Stop()
        {
            _logger?.LogMethodCall(caller: this);

            _isRunning = false;
            _datesQueue.Clear();
        }

        private async Task GenerateAndStoreData(CancellationToken cancellationToken)
        {
            var random = new Random(DateTime.Now.Millisecond);

            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_datesQueue.TryDequeue(out var date))
                    {
                        ThrowIfDisposed();

                        var data = new GeneratedDataEventArgs(date.RoundSeconds(), _dataTemplate[random.Next(_dataTemplate.Length)]);

                        _logger?.LogMethodCall(caller: this, args: data);

                        await _storage!.SaveDataAsync(data.Date, data.Data, cancellationToken).ConfigureAwait(false);

                        ThrowIfDisposed();

                        var delegates = DataGenerated?.GetInvocationList()?.Cast<EventHandler<GeneratedDataEventArgs>>();

                        if (delegates?.Any() == true)
                        {
                            var self = this as IDataGenerator;

                            foreach (var @delegate in delegates)
                            {
                                if (cancellationToken.IsCancellationRequested) return;

                                try
                                {
                                    @delegate?.Invoke(self, data);
                                }
                                catch (Exception error) when (_logger!.FilterAndLogError(error))
                                {
                                }
                            }
                        }
                    }
                    else if (_isRunning)
                    {
                        await Task.Delay(_generatorResolution * 1000, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (OperationCanceledException) { return; }
                catch (ObjectDisposedException) { return; }
                catch (Exception e) when (_logger?.FilterAndLogError(LogLevel.Critical, e) == true)
                {
                }
            }
        }

        protected override void DisposeConcrete()
        {
            _isRunning = false;

            try
            {
                using (_cancellationTokenSource)
                {
                    _cancellationTokenSource.Cancel();
                    _datesQueue.Clear();

                    _timer!.Tick -= Timer_Tick;
                }
            }
            catch (Exception e) when(_logger?.FilterAndLogError(LogLevel.Critical, e) == true)
            {
            }
            finally
            {
                DataGenerated = null;
                _timer = null;
                _storage = null;
                _logger = null;
            }
        }
    }
}

