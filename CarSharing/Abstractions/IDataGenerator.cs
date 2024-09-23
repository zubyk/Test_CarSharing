namespace CarSharing.Abstractions
{
    public interface IDataGenerator
    {
        event EventHandler<GeneratedDataEventArgs>? DataGenerated;
        bool IsRunning { get; }

        void Start();
        void Stop();
    }

    public class GeneratedDataEventArgs(DateTime date, string data) : EventArgs
    {
        public DateTime Date { get => date; }
        public string Data { get => data; }

        public override string ToString()
        {
            return $"[GeneratedDataEventArgs({date}, {data})]";
        }
    }

    public interface IDriversDataGenerator: IDataGenerator
    {

    }

    public interface ICarsDataGenerator : IDataGenerator
    {

    }
}