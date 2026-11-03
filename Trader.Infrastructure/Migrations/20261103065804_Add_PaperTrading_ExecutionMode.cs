using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_PaperTrading_ExecutionMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExecutionMode",
                schema: "trader",
                table: "TradingRuns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaper",
                schema: "trader",
                table: "Positions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaper",
                schema: "trader",
                table: "OrderIntents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaper",
                schema: "trader",
                table: "BrokerOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionMode",
                schema: "trader",
                table: "TradingRuns");

            migrationBuilder.DropColumn(
                name: "IsPaper",
                schema: "trader",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "IsPaper",
                schema: "trader",
                table: "OrderIntents");

            migrationBuilder.DropColumn(
                name: "IsPaper",
                schema: "trader",
                table: "BrokerOrders");
        }
    }
}
