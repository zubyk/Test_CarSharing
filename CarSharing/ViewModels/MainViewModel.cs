using CarSharing.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace CarSharing.ViewModels
{
    internal class MainViewModel: ViewModelBase
    {
        private IDataGenerator? _cars;
        private IDataGenerator? _drivers;
        private InvokeUIUpdate? _uiUpdateDelegate;
        private ILogger<MainViewModel>? _logger;

        public ICommandBase ToggleDriversGeneration { get; }
        public ICommandBase ToggleCarsGeneration { get; }

        public bool IsCarsRunning => _cars?.IsRunning == true;
        public bool IsDriversRunning => _drivers?.IsRunning == true;

        public ObservableCollection<CarDriverEntity> Items { get; }

        public MainViewModel(IDataGenerator cars, IDataGenerator drivers, ILogger<MainViewModel> logger)
        {
            ArgumentNullException.ThrowIfNull(cars);
            ArgumentNullException.ThrowIfNull(drivers);
            ArgumentNullException.ThrowIfNull(logger);

            try
            {
                _cars = cars;
                _drivers = drivers;
                _logger = logger;

                ToggleCarsGeneration = new CommandBase(() => ToggleDataGeneration(_cars, nameof(IsCarsRunning)), () => !IsDisposed);
                ToggleDriversGeneration = new CommandBase(() => ToggleDataGeneration(_drivers, nameof(IsDriversRunning)), () => !IsDisposed);

                Items = [];

                _cars!.DataGenerated += Cars_DataGenerated;
                _drivers!.DataGenerated += Drivers_DataGenerated;   
            }
            catch
            {
                (ToggleCarsGeneration as CommandBase)?.Dispose();
                (ToggleDriversGeneration as CommandBase)?.Dispose();
                
                _cars = null;
                _drivers = null;
                _uiUpdateDelegate = null;
                _logger = null;
                throw;
            }
        }

        public void SetUiUpdateDelegate(InvokeUIUpdate? uiUpdateDelegate)
        {
            ThrowIfDisposed();

            _uiUpdateDelegate = uiUpdateDelegate;
        }

        private void ToggleDataGeneration(IDataGenerator dataGenerator, string propertyName)
        {
            if (dataGenerator is not null)
            {
                _logger?.LogMethodCall(caller: this, args: propertyName);

                if (dataGenerator.IsRunning)
                {
                    dataGenerator.Stop();
                }
                else
                {
                    dataGenerator.Start();
                }

                RaisePropertyChanged(propertyName);
            }
        }

        private void Cars_DataGenerated(object? sender, GeneratedDataEventArgs e)
        {
            if (IsDisposed) return;
            
            lock (Items)
            {
                _uiUpdateDelegate?.Invoke(() =>
                {
                    var existsItem = FindItem(e.Date, out var itemIndex);

                    if (existsItem is not null)
                    {
                        existsItem.UpdateCar(e.Data);
                        
                        _logger?.LogEntitiesTimeMatch(existsItem);
                    }
                    else
                    {
                        Items.Insert(itemIndex, new CarDriverEntity(e.Date, e.Data, null));
                    }
                });
            }
        }

        private void Drivers_DataGenerated(object? sender, GeneratedDataEventArgs e)
        {
            if (IsDisposed) return;

            lock (Items)
            {
                _uiUpdateDelegate?.Invoke(() =>
                {
                    var existsItem = FindItem(e.Date, out var itemIndex);

                    if (existsItem is not null)
                    {
                        existsItem.UpdateDriver(e.Data);

                        _logger?.LogEntitiesTimeMatch(existsItem);
                    }
                    else
                    {
                        Items.Insert(itemIndex, new CarDriverEntity(e.Date, null, e.Data));
                    }
                });
            }
        }

        private CarDriverEntity? FindItem(DateTime date, out int indexOf)
        {
            for (indexOf = 0; indexOf < 10 && indexOf < Items.Count; indexOf++)
            {
                var item = Items[indexOf];

                if (item.Date == date)
                {
                    return item;
                }
                else if (item.Date > date)
                {
                    continue;
                }
                else
                {
                    return null;
                }
            }

            indexOf = 0;
            return null;
        }

        protected override void DisposeConcrete()
        {
            try
            {
                (ToggleCarsGeneration as CommandBase)?.Dispose();
                (ToggleDriversGeneration as CommandBase)?.Dispose();

                _cars!.DataGenerated -= Cars_DataGenerated;
                _drivers!.DataGenerated -= Cars_DataGenerated;

                var items = Items.ToArray();

                Items.Clear();

                foreach (var item in items)
                {
                    item.Dispose();
                }
            }
            catch (Exception e) when (_logger?.FilterAndLogError(e) == true)
            {
            }
            finally
            {
                _cars = null;
                _drivers = null;
                _uiUpdateDelegate = null;
                _logger= null;
            }
            
            base.DisposeConcrete();
        }
    }
}
