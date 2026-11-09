using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_PerAccountCapital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapitalAllocationMode",
                schema: "trader",
                table: "TradingRuns");

            migrationBuilder.DropColumn(
                name: "FixedCapitalAmount",
                schema: "trader",
                table: "TradingRuns");

            migrationBuilder.DropColumn(
                name: "FixedCapitalCurrency",
                schema: "trader",
                table: "TradingRuns");

            migrationBuilder.DropColumn(
                name: "AllocationPercent",
                schema: "trader",
                table: "TradingRunAccounts");

            migrationBuilder.AddColumn<int>(
                name: "CapitalMode",
                schema: "trader",
                table: "TradingRunAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedCapitalAmount",
                schema: "trader",
                table: "TradingRunAccounts",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FixedCapitalCurrency",
                schema: "trader",
                table: "TradingRunAccounts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentOfBuyPower",
                schema: "trader",
                table: "TradingRunAccounts",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapitalMode",
                schema: "trader",
                table: "TradingRunAccounts");

            migrationBuilder.DropColumn(
                name: "FixedCapitalAmount",
                schema: "trader",
                table: "TradingRunAccounts");

            migrationBuilder.DropColumn(
                name: "FixedCapitalCurrency",
                schema: "trader",
                table: "TradingRunAccounts");

            migrationBuilder.DropColumn(
                name: "PercentOfBuyPower",
                schema: "trader",
                table: "TradingRunAccounts");

            migrationBuilder.AddColumn<int>(
                name: "CapitalAllocationMode",
                schema: "trader",
                table: "TradingRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedCapitalAmount",
                schema: "trader",
                table: "TradingRuns",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FixedCapitalCurrency",
                schema: "trader",
                table: "TradingRuns",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AllocationPercent",
                schema: "trader",
                table: "TradingRunAccounts",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 100m);
        }
    }
}
