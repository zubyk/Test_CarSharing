using CarSharing.Abstractions;
using CarSharing.Services;
using Microsoft.Extensions.Logging;

namespace CarSharing.Tests
{
    [TestClass]
    public class DataGeneratorServiceTests
    {
        internal class TestDataStorage(Func<Task>? action = null) : IDataStorage
        {
            public Task SaveDataAsync(DateTime date, string value, CancellationToken cancellationToken)
            {
                return action?.Invoke();
            }
        }

        internal class TestDataGeneratorConfiguration : DataGeneratorConfiguration
        {
        }

        internal class TestTimeSignal : ITimeSignal
        {
            public event EventHandler<DateTime> Tick;

            public void Signal(DateTime date)
            {
                Tick?.Invoke(this as ITimeSignal, date);
            }
        }

        private readonly ILogger<DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>> _logger = LoggerFactory.Create(c => c.AddConsole()).CreateLogger<DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>>();


        [TestMethod]
        public void ctor_NullCheck()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>(null, null, null, null);
            }, $"{nameof(TimerService)}.ctor must throw {nameof(ArgumentNullException)} on all null");

           
            var timer = new TestTimeSignal();
            var storage = new TestDataStorage();

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>(null, timer, storage, _logger);
            }, $"{nameof(TimerService)}.ctor must throw {nameof(ArgumentNullException)} on {nameof(TestDataGeneratorConfiguration)} null");

            var config = new TestDataGeneratorConfiguration();

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>(config, null, storage, _logger);
            }, $"{nameof(TimerService)}.ctor must throw {nameof(ArgumentNullException)} on {nameof(TestTimeSignal)} null");

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>(config, timer, null, _logger);
            }, $"{nameof(TimerService)}.ctor must throw {nameof(ArgumentNullException)} on {nameof(TestDataStorage)} null");

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _ = new DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>(config, timer, storage, null);
            }, $"{nameof(TimerService)}.ctor must throw {nameof(ArgumentNullException)} on {nameof(ILogger<DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>>)} null");
        }

        [TestMethod]
        [DataRow(-1, false, null)]
        [DataRow(0, false, null)]
        [DataRow(1, false, null)]
        [DataRow(1, false, "")]
        [DataRow(1, true, "data1", "data1")]
        [DataRow(1, true, "data1", "data2")]
        public void ctor_ConfigurationCheck(int generatorResolutionSeconds, bool isSuccess = false, params string[] args)
        {

            var timer = new TestTimeSignal();
            var storage = new TestDataStorage();
            
            var config = new TestDataGeneratorConfiguration()
            {
                GeneratorResolutionSeconds = generatorResolutionSeconds,
                Data = args,
            };

            if (isSuccess)
            {
                try
                {
                    _ = new DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>(config, timer, storage, _logger);
                }
                catch (Exception e)
                {
                    Assert.Fail($"{nameof(DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>)}.ctor throw {e.GetType()}: {e.Message}");
                }
            }
            else
            {
                Assert.ThrowsException<ArgumentException>(() =>
                {
                    _ = new DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>(config, timer, storage, _logger);
                }, $"{nameof(TimerService)}.ctor must throw {nameof(ArgumentException)}");
            }
        }

        [TestMethod]
        public void DisposedStartStopCheck()
        {
            var timer = new TestTimeSignal();
            var storage = new TestDataStorage();

            var config = new TestDataGeneratorConfiguration()
            {
                GeneratorResolutionSeconds = 1,
                Data = [ "data1", "data2" ],
            };

            var generator = new DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>(config, timer, storage, _logger);
            generator.Dispose();
            
            try
            {
                generator.Stop();
            }
            catch (Exception e)
            {
                Assert.Fail($"Stop throw {e.GetType()}: {e.Message}");
                
            }

            Assert.ThrowsException<ObjectDisposedException>(() =>
            {
                generator.Start();
            }, $"{nameof(DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>)} must throw {nameof(ObjectDisposedException)}");

        }

        [TestMethod]
        [DataRow(3)]
        [DataRow(2)]
        public async Task GenerationCountCheck(int generatorResolutionSeconds)
        {
            int generatedData = 0;
            int storedData = 0;

            var timer = new TestTimeSignal();
            var storage = new TestDataStorage(() => { storedData++; return Task.CompletedTask; });

            var config = new TestDataGeneratorConfiguration()
            {
                GeneratorResolutionSeconds = generatorResolutionSeconds,
                Data = [ "data1", "data2" ],
            };

            var generator = new DataGeneratorService<TestDataGeneratorConfiguration, TestDataStorage>(config, timer, storage, _logger);

            GenerateTick(generatorResolutionSeconds);
            GenerateTick(generatorResolutionSeconds);
            GenerateTick(generatorResolutionSeconds);

            await Task.Delay(generatorResolutionSeconds * 1000 + 500);

            Assert.IsTrue(storedData == 0, $"Stored {storedData} events on stopped state");

            generator.Start();

            await Task.Delay(generatorResolutionSeconds * 1000 + 500);

            Assert.IsTrue(storedData == 0, $"Stored {storedData} events after first start");


            generatedData = 0;
            storedData = 0;

            GenerateTick(generatorResolutionSeconds);
            GenerateTick(generatorResolutionSeconds);
            GenerateTick(generatorResolutionSeconds);

            await Task.Delay(generatorResolutionSeconds * 1000 + 500);

            Assert.IsTrue(storedData == generatedData, $"Generated {generatedData} events but stored {storedData}");


            storedData = 0;

            GenerateTick(generatorResolutionSeconds + 1);
            GenerateTick(generatorResolutionSeconds + 1);
            GenerateTick(generatorResolutionSeconds + 1);

            await Task.Delay(generatorResolutionSeconds * 1000 + 500);

            Assert.IsTrue(storedData == 0, $"Generated {storedData} events with invalid resolution {generatorResolutionSeconds + 1}");


            generatedData = 0;
            storedData = 0;
            generator.Dispose();

            GenerateTick(generatorResolutionSeconds);
            GenerateTick(generatorResolutionSeconds);
            GenerateTick(generatorResolutionSeconds);

            await Task.Delay(generatorResolutionSeconds * 1000 + 500);

            Assert.IsTrue(storedData == 0, $"Stored {storedData} events after {nameof(generator.Dispose)} call");


            void GenerateTick(int second)
            {
                var date = DateTime.Now.Date.AddSeconds(second);

                generatedData++;
                timer.Signal(date);
            }
        }
    }
}