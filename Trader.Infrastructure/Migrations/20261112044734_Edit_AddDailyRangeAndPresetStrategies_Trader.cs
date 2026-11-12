using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_AddDailyRangeAndPresetStrategies_Trader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultPrice",
                schema: "trader",
                table: "InstrumentOrderPresets");

            migrationBuilder.DropColumn(
                name: "DefaultQuantity",
                schema: "trader",
                table: "InstrumentOrderPresets");

            migrationBuilder.AddColumn<decimal>(
                name: "DailyPriceRangePercent",
                schema: "trader",
                table: "Instruments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 3.0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedPrice",
                schema: "trader",
                table: "InstrumentOrderPresets",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FixedQuantity",
                schema: "trader",
                table: "InstrumentOrderPresets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceStrategy",
                schema: "trader",
                table: "InstrumentOrderPresets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantityStrategy",
                schema: "trader",
                table: "InstrumentOrderPresets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyPriceRangePercent",
                schema: "trader",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "FixedPrice",
                schema: "trader",
                table: "InstrumentOrderPresets");

            migrationBuilder.DropColumn(
                name: "FixedQuantity",
                schema: "trader",
                table: "InstrumentOrderPresets");

            migrationBuilder.DropColumn(
                name: "PriceStrategy",
                schema: "trader",
                table: "InstrumentOrderPresets");

            migrationBuilder.DropColumn(
                name: "QuantityStrategy",
                schema: "trader",
                table: "InstrumentOrderPresets");

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultPrice",
                schema: "trader",
                table: "InstrumentOrderPresets",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "DefaultQuantity",
                schema: "trader",
                table: "InstrumentOrderPresets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
