using Microsoft.Extensions.Logging;
using CarSharing.Extensions;
using Timer = System.Timers.Timer;
using CarSharing.Abstractions;

namespace CarSharing.Services
{
    internal class TimerService: SafeDisposable, ITimeSignal
    {
        private readonly Timer? _timer;

        private ILogger<TimerService>? _logger;

        public event EventHandler<DateTime>? Tick;

        public TimerService(TimeSignalConfiguration configuration, ILogger<TimerService> logger)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(logger);

            if (configuration.TimerResolutionSeconds <= 0) throw new ArgumentException($"{nameof(configuration.TimerResolutionSeconds)} must be greater than zero", nameof(configuration));

            _logger = logger;

            try
            {
                _timer = new(TimeSpan.FromSeconds(configuration.TimerResolutionSeconds))
                {
                    AutoReset = true
                };

                _timer.Elapsed += TimerService_TimeGenerated;

                _timer.Start();
            }
            catch
            {
                _logger = null;

                if (_timer is not null)
                {
                    using (_timer)
                    {
                        _timer.Elapsed -= TimerService_TimeGenerated;
                    }

                    _timer = null;
                }

                throw;
            }
        }

        private void TimerService_TimeGenerated(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (IsDisposed) return;

            _logger?.LogMethodCall(caller: this, args: e.SignalTime);

            var delegates = Tick?.GetInvocationList()?.Cast<EventHandler<DateTime>>();

            if (delegates?.Any() == true)
            {
                var self = this as ITimeSignal;
                var date = e.SignalTime.RoundSeconds();

                foreach (var @delegate in delegates)
                {
                    if (IsDisposed) return;

                    try
                    {
                        @delegate?.Invoke(self, date);
                    }
                    catch (Exception error) when (_logger?.FilterAndLogError(error) == true)
                    {
                    }
                }
            }
        }

        protected override void DisposeConcrete()
        {
            _logger?.LogMethodCall(caller: this);

            try
            {
                using (_timer)
                {
                    _timer!.Elapsed -= TimerService_TimeGenerated;
                    _timer!.Stop();
                }
            }
            catch (Exception e) when (_logger?.FilterAndLogError(LogLevel.Critical, e) == true)
            {
            }
            finally 
            {
                Tick = null;
                _logger = null;
            }
        }
    }
}
