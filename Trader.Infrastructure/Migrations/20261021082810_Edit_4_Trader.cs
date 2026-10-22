using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_4_Trader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MarketSnapshotId",
                schema: "trader",
                table: "StrategyEvaluations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MarketSnapshots",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SymbolName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StateCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    LastTradedPrice = table.Column<long>(type: "bigint", nullable: false),
                    ClosingPrice = table.Column<long>(type: "bigint", nullable: false),
                    PreviousDayClosingPrice = table.Column<long>(type: "bigint", nullable: false),
                    PriceVar = table.Column<double>(type: "float", nullable: false),
                    FirstTradedPrice = table.Column<long>(type: "bigint", nullable: false),
                    HighPrice = table.Column<long>(type: "bigint", nullable: false),
                    LowPrice = table.Column<long>(type: "bigint", nullable: false),
                    HighAllowedPrice = table.Column<long>(type: "bigint", nullable: false),
                    LowAllowedPrice = table.Column<long>(type: "bigint", nullable: false),
                    PriceTickSize = table.Column<long>(type: "bigint", nullable: false),
                    BestBuyPrice = table.Column<long>(type: "bigint", nullable: false),
                    BestBuyQuantity = table.Column<long>(type: "bigint", nullable: false),
                    BestSellPrice = table.Column<long>(type: "bigint", nullable: false),
                    BestSellQuantity = table.Column<long>(type: "bigint", nullable: false),
                    BuyRatio = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    SellRatio = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    MinValidBuyVolume = table.Column<long>(type: "bigint", nullable: false),
                    MaxValidBuyVolume = table.Column<long>(type: "bigint", nullable: false),
                    MinValidSellVolume = table.Column<long>(type: "bigint", nullable: false),
                    MaxValidSellVolume = table.Column<long>(type: "bigint", nullable: false),
                    TotalTradeValue = table.Column<long>(type: "bigint", nullable: false),
                    TotalNumberOfTrades = table.Column<long>(type: "bigint", nullable: false),
                    TotalNumberOfSharesTraded = table.Column<long>(type: "bigint", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketSnapshotRecord", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StrategyEval_MarketSnapshotId",
                schema: "trader",
                table: "StrategyEvaluations",
                column: "MarketSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketSnapshot_Isin_Time",
                schema: "trader",
                table: "MarketSnapshots",
                columns: new[] { "Isin", "CapturedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketSnapshot_Time",
                schema: "trader",
                table: "MarketSnapshots",
                column: "CapturedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MarketSnapshots_Id",
                schema: "trader",
                table: "MarketSnapshots",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketSnapshots",
                schema: "trader");

            migrationBuilder.DropIndex(
                name: "IX_StrategyEval_MarketSnapshotId",
                schema: "trader",
                table: "StrategyEvaluations");

            migrationBuilder.DropColumn(
                name: "MarketSnapshotId",
                schema: "trader",
                table: "StrategyEvaluations");
        }
    }
}
