using CarSharing.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarSharing.Migrations
{
    /// <inheritdoc />
    public partial class CreateViewMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"
                CREATE VIEW IF NOT EXISTS {DbDataContext.CarDriversViewName} AS
                SELECT IFNULL(c.Date, d.Date) AS Date, c.Name AS Car, d.Name AS Driver
                FROM Cars AS c FULL JOIN Drivers AS d
                    ON c.Date = d.Date
                ORDER BY 1 DESC;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"DROP TABLE IF EXISTS {DbDataContext.CarDriversViewName};");
        }
    }
}
