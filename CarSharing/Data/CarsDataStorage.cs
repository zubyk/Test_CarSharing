using CarSharing.Abstractions;

namespace CarSharing.Data
{
    internal class CarsDataStorage : IDataStorage
    {
        private readonly DbDataContext _dbContext;

        public CarsDataStorage(DbDataContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _dbContext = context;
        }

        public async Task SaveDataAsync(DateTime date, string value, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
           
            await _dbContext.Cars.AddAsync(new CarModel()
            {
                Date = date,
                Name = value,
            }, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
    }
}