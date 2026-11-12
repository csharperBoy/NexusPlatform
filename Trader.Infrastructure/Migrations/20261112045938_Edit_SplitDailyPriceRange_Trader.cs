using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_SplitDailyPriceRange_Trader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyPriceRangePercent",
                schema: "trader",
                table: "Instruments");

            migrationBuilder.AddColumn<decimal>(
                name: "DailyPriceRangeDownPercent",
                schema: "trader",
                table: "Instruments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                defaultValue: 3.0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DailyPriceRangeUpPercent",
                schema: "trader",
                table: "Instruments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                defaultValue: 3.0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyPriceRangeDownPercent",
                schema: "trader",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "DailyPriceRangeUpPercent",
                schema: "trader",
                table: "Instruments");

            migrationBuilder.AddColumn<decimal>(
                name: "DailyPriceRangePercent",
                schema: "trader",
                table: "Instruments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 3.0m);
        }
    }
}
