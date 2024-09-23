namespace CarSharing.ViewModels
{
    internal class CarDriverEntity(DateTime date, string? car, string? driver) : Notify
    {
        public DateTime Date { get; } = date;
        public string? Car { get; set; } = car;
        public string? Driver { get; set; } = driver;

        internal void UpdateCar(string data)
        {
            if (!string.Equals(data, Car))
            {
                Car = data;
                RaisePropertyChanged(nameof(Car));
            }
        }

        internal void UpdateDriver(string data)
        {
            if (!string.Equals(data, Driver))
            {
                Driver = data;
                RaisePropertyChanged(nameof(Driver));
            }
        }

        public override string ToString()
        {
            return $"[{nameof(CarDriverEntity)}({Date}, {Car}, {Driver})]";
        }
    }
}
