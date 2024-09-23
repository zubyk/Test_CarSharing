namespace CarSharing.Abstractions
{
    public interface IDataStorage
    {
        public Task SaveDataAsync(DateTime date, string value, CancellationToken cancellationToken);
    }
}