using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_InstrumentCommissionAndLimits_And_StrengthMultiplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "StrengthMultiplier",
                schema: "trader",
                table: "TradingRuns",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 1.5m);

            migrationBuilder.AddColumn<decimal>(
                name: "BuyCommissionRate",
                schema: "trader",
                table: "Instruments",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0.0037m);

            migrationBuilder.AddColumn<long>(
                name: "MaxOrderQuantity",
                schema: "trader",
                table: "Instruments",
                type: "bigint",
                nullable: false,
                defaultValue: 100000L);

            migrationBuilder.AddColumn<long>(
                name: "MinOrderQuantity",
                schema: "trader",
                table: "Instruments",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<long>(
                name: "MinOrderValueRial",
                schema: "trader",
                table: "Instruments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SellCommissionRate",
                schema: "trader",
                table: "Instruments",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0.0037m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StrengthMultiplier",
                schema: "trader",
                table: "TradingRuns");

            migrationBuilder.DropColumn(
                name: "BuyCommissionRate",
                schema: "trader",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "MaxOrderQuantity",
                schema: "trader",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "MinOrderQuantity",
                schema: "trader",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "MinOrderValueRial",
                schema: "trader",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "SellCommissionRate",
                schema: "trader",
                table: "Instruments");
        }
    }
}
