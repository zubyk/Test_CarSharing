using CarSharing.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CarSharing.Data
{
    internal class DriversDataStorage : IDataStorage
    {
        private readonly DbDataContext _dbContext;

        public DriversDataStorage(DbDataContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _dbContext = context;
        }

        public async Task SaveDataAsync(DateTime date, string value, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
           
            await _dbContext.Drivers.AddAsync(new DriverModel()
            {
                Date = date,
                Name = value,
            }, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
    }
}