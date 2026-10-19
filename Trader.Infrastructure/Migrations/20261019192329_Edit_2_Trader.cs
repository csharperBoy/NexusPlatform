using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_2_Trader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "StopLossPrice",
                schema: "trader",
                table: "OrderIntents",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TakeProfitPrice",
                schema: "trader",
                table: "OrderIntents",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderIntent_SourceSignalId",
                schema: "trader",
                table: "OrderIntents",
                column: "SourceSignalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderIntent_SourceSignalId",
                schema: "trader",
                table: "OrderIntents");

            migrationBuilder.DropColumn(
                name: "StopLossPrice",
                schema: "trader",
                table: "OrderIntents");

            migrationBuilder.DropColumn(
                name: "TakeProfitPrice",
                schema: "trader",
                table: "OrderIntents");
        }
    }
}
