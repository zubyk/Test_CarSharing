using CarSharing.Abstractions;
using CarSharing.Services;
using Microsoft.Extensions.Logging;

namespace CarSharing.Tests
{
    [TestClass]
    public class TimerServiceTests
    {
        private readonly ILogger<TimerService> _logger = LoggerFactory.Create(c => c.AddConsole()).CreateLogger<TimerService>();

        [TestMethod]
        public void ctor_NullCheck()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new TimerService(null, null);
            }, $"{nameof(TimerService)}.ctor must throw {nameof(ArgumentNullException)} on all null");

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new TimerService(new TimeSignalConfiguration(), null);
            }, $"{nameof(TimerService)}.ctor must throw {nameof(ArgumentNullException)} on {nameof(TimeSignalConfiguration)} null");

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new TimerService(null, _logger);
            }, $"{nameof(TimerService)}.ctor must throw {nameof(ArgumentNullException)} on {nameof(ILogger<TimerService>)} null");
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(1, true)]
        public void ctor_ConfigurationCheck(int timerResolutionSeconds, bool isSuccess = false)
        {
           
            var config = new TimeSignalConfiguration()
            { 
                TimerResolutionSeconds = timerResolutionSeconds 
            };

            if (isSuccess)
            {
                try
                {
                    _ = new TimerService(config, _logger);
                }
                catch (Exception e)
                {
                    Assert.Fail($"{nameof(TimerService)}.ctor throw {e.GetType()}: {e.Message}");
                }
            }
            else
            {
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    _ = new TimerService(config, _logger);
                }, $"{nameof(TimerService)}.ctor must throw {nameof(ArgumentException)}");
            }
        }

        [TestMethod]
        [DataRow(1, 10)]
        [DataRow(2, 30)]
        [DataRow(3, 15)]
        [DataRow(4, 8)]
        public async Task Tick_CountCheck(int timerResolutionSeconds, int testPeriodSeconds)
        {
            var config = new TimeSignalConfiguration()
            {
                TimerResolutionSeconds = timerResolutionSeconds
            };

            int eventGenerated = 0;

            using (var service = new TimerService(config, _logger))
            {
                service.Tick += (s, e) =>
                {
                    eventGenerated++;
                };

                await Task.Delay(testPeriodSeconds * 1000 + timerResolutionSeconds);
            }

            var diff = eventGenerated - testPeriodSeconds / timerResolutionSeconds;

            Assert.IsTrue(diff >= -1 && diff <= 1 , $"Generated {eventGenerated} events, expected {testPeriodSeconds / timerResolutionSeconds} or same");
        }

        [TestMethod]
        [DataRow(1, 5)]
        [DataRow(2, 10)]
        [DataRow(3, 12)]
        [DataRow(4, 8)]
        public async Task Tick_StopOnDisposingCheck(int timerResolutionSeconds, int testPeriodSeconds)
        {
            var config = new TimeSignalConfiguration()
            {
                TimerResolutionSeconds = timerResolutionSeconds
            };

            int eventGenerated = 0;
            int beforeDisposed = 0;

            using (var service = new TimerService(config, _logger))
            {
                service.Tick += (s, e) =>
                {
                    eventGenerated++;
                };

                await Task.Delay(testPeriodSeconds * 1000);
            }

            beforeDisposed = eventGenerated;

            await Task.Delay(timerResolutionSeconds * 2 * 1000);

            Assert.IsTrue(beforeDisposed == eventGenerated, $"Generated {eventGenerated - beforeDisposed} events after disposing, expected 0");
        }

        [TestMethod]
        [DataRow(1, 5, 2)]
        [DataRow(2, 10, 2)]
        [DataRow(3, 12, 3)]
        [DataRow(4, 8, 1)]
        public async Task Tick_FallbackCheck(int timerResolutionSeconds, int testPeriodSeconds, int failEveryNTick)
        {
            var config = new TimeSignalConfiguration()
            {
                TimerResolutionSeconds = timerResolutionSeconds
            };

            int eventGenerated = 0;
            int failedEvents = 0;

            using (var service = new TimerService(config, _logger))
            {
                service.Tick += (s, e) =>
                {
                    eventGenerated++;

                    if (eventGenerated % failEveryNTick == 0)
                    {
                        failedEvents++;
                        throw new NotImplementedException();
                    }
                };

                await Task.Delay(testPeriodSeconds * 1000 + timerResolutionSeconds);
            }

            var diff = eventGenerated - testPeriodSeconds / timerResolutionSeconds;
            var failedDiff = failedEvents - testPeriodSeconds / timerResolutionSeconds / failEveryNTick;

            Assert.IsTrue(diff >= -1 && diff <= 1, $"Generated {eventGenerated} events, expected {testPeriodSeconds / timerResolutionSeconds} or same");
            Assert.IsTrue(failedDiff >= -1 && failedDiff <= 1, $"Generated {failedEvents} failed events, expected {testPeriodSeconds / timerResolutionSeconds / failEveryNTick} or same");
        }
    }
}