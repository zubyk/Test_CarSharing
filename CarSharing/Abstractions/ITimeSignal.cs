namespace CarSharing.Abstractions
{
    public interface ITimeSignal
    {
        public event EventHandler<DateTime> Tick;
    }
}