using Microsoft.EntityFrameworkCore;

namespace CarSharing.Data
{
    internal class DbDataContext : DbContext
    {
        internal static readonly string CarDriversViewName = "CarDriversViewName";

        public DbSet<CarModel> Cars { get; set; }
        public DbSet<DriverModel> Drivers { get; set; }

        public DbSet<CarDriverModel> CarDriversView { get; set; }
                

        public DbDataContext(DbContextOptions options): base(options)
        {
            if (Database.GetPendingMigrations().Any())
            {
                Database.Migrate();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<CarDriverModel>()
                .ToView(CarDriversViewName)
                .HasKey(t => t.Date);
        }
    }
}
