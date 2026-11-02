using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Backtest_Infrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BacktestDailyEquities",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BacktestRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Equity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Cash = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PositionValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DailyReturnPct = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacktestDailyEquity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BacktestRuns",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StrategyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StrategyConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryTimeFrame = table.Column<int>(type: "int", nullable: false),
                    TargetCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CapitalAllocationMode = table.Column<int>(type: "int", nullable: false),
                    FixedCapitalAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    StrengthMultiplier = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    MinDeltaPercent = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    StopLossPercentage = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    StopLossPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    TakeProfitPercentage = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    TakeProfitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DefaultOrderMode = table.Column<int>(type: "int", nullable: false),
                    OnMarketClose = table.Column<int>(type: "int", nullable: false),
                    OnStop = table.Column<int>(type: "int", nullable: false),
                    ConfigSnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InitialCapital = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SlippagePercent = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    SubCandleTimeFrame = table.Column<int>(type: "int", nullable: true),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FinalEquity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalReturnPct = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    MaxDrawdownPct = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    SharpeRatio = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    WinRate = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    TotalTrades = table.Column<int>(type: "int", nullable: false),
                    WinningTrades = table.Column<int>(type: "int", nullable: false),
                    LosingTrades = table.Column<int>(type: "int", nullable: false),
                    ProfitFactor = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    AvgWin = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AvgLoss = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalCommission = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalSlippage = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacktestRun", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BacktestTrades",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BacktestRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEntryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalEntryQuantity = table.Column<long>(type: "bigint", nullable: false),
                    WeightedAvgEntryPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalBuyCommission = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EntryFillsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExitTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExitQuantity = table.Column<long>(type: "bigint", nullable: false),
                    TotalSellCommission = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExitReason = table.Column<int>(type: "int", nullable: false),
                    GrossPnL = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    NetPnL = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    NetPnLPct = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    HoldingDurationSeconds = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacktestTrade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricalDataCollectionJobs",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsinsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeFramesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCombinations = table.Column<int>(type: "int", nullable: false),
                    CompletedCombinations = table.Column<int>(type: "int", nullable: false),
                    TotalCandlesCollected = table.Column<int>(type: "int", nullable: false),
                    ProgressPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CurrentIsin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CurrentTimeFrame = table.Column<int>(type: "int", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LogMessagesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalDataCollectionJob", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BacktestSymbols",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BacktestRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AllocationPercentage = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacktestSymbol", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BacktestSymbols_BacktestRuns_BacktestRunId",
                        column: x => x.BacktestRunId,
                        principalSchema: "trader",
                        principalTable: "BacktestRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BacktestDailyEquities_Id",
                schema: "trader",
                table: "BacktestDailyEquities",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_BacktestDailyEquity_Run_Date",
                schema: "trader",
                table: "BacktestDailyEquities",
                columns: new[] { "BacktestRunId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRun_CreatedAt",
                schema: "trader",
                table: "BacktestRuns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRun_CreatedBy",
                schema: "trader",
                table: "BacktestRuns",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRun_ModifiedAt",
                schema: "trader",
                table: "BacktestRuns",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRun_ModifiedBy",
                schema: "trader",
                table: "BacktestRuns",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRun_OwnerOrgUnit",
                schema: "trader",
                table: "BacktestRuns",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRun_OwnerPerson",
                schema: "trader",
                table: "BacktestRuns",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRun_ScopedLookup",
                schema: "trader",
                table: "BacktestRuns",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRun_SourceRun",
                schema: "trader",
                table: "BacktestRuns",
                column: "SourceRunId");

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRun_Status",
                schema: "trader",
                table: "BacktestRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BacktestRuns_Id",
                schema: "trader",
                table: "BacktestRuns",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BacktestSymbols_Id",
                schema: "trader",
                table: "BacktestSymbols",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_BacktestSymbol_Run_Isin",
                schema: "trader",
                table: "BacktestSymbols",
                columns: new[] { "BacktestRunId", "Isin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BacktestTrade_Run_ExitTime",
                schema: "trader",
                table: "BacktestTrades",
                columns: new[] { "BacktestRunId", "ExitTime" });

            migrationBuilder.CreateIndex(
                name: "IX_BacktestTrade_Run_Isin",
                schema: "trader",
                table: "BacktestTrades",
                columns: new[] { "BacktestRunId", "Isin" });

            migrationBuilder.CreateIndex(
                name: "IX_BacktestTrade_Run_NetPnL",
                schema: "trader",
                table: "BacktestTrades",
                columns: new[] { "BacktestRunId", "NetPnL" });

            migrationBuilder.CreateIndex(
                name: "IX_BacktestTrades_Id",
                schema: "trader",
                table: "BacktestTrades",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalDataCollectionJob_CreatedAt",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalDataCollectionJob_CreatedBy",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalDataCollectionJob_ModifiedAt",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalDataCollectionJob_ModifiedBy",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalDataCollectionJob_Status",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalDataCollectionJobs_Id",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BacktestDailyEquities",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "BacktestSymbols",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "BacktestTrades",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "HistoricalDataCollectionJobs",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "BacktestRuns",
                schema: "trader");
        }
    }
}
