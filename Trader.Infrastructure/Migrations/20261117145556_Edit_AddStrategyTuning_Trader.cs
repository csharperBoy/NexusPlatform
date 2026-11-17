using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_AddStrategyTuning_Trader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StrategyTuningResults",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TuningRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Generation = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RuntimeMs = table.Column<long>(type: "bigint", nullable: false),
                    AvgReturnArithmeticYearly = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AvgReturnCompoundYearly = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ValidWindowsYearly = table.Column<int>(type: "int", nullable: true),
                    TotalTradesYearly = table.Column<int>(type: "int", nullable: true),
                    AvgReturnArithmeticSemi = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AvgReturnCompoundSemi = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ValidWindowsSemi = table.Column<int>(type: "int", nullable: true),
                    TotalTradesSemi = table.Column<int>(type: "int", nullable: true),
                    AvgReturnArithmeticQuarter = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AvgReturnCompoundQuarter = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ValidWindowsQuarter = table.Column<int>(type: "int", nullable: true),
                    TotalTradesQuarter = table.Column<int>(type: "int", nullable: true),
                    AvgReturnArithmeticMonthly = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    AvgReturnCompoundMonthly = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ValidWindowsMonthly = table.Column<int>(type: "int", nullable: true),
                    TotalTradesMonthly = table.Column<int>(type: "int", nullable: true),
                    IsTop10InYearly = table.Column<bool>(type: "bit", nullable: false),
                    IsTop10InSemi = table.Column<bool>(type: "bit", nullable: false),
                    IsTop10InQuarterly = table.Column<bool>(type: "bit", nullable: false),
                    IsTop10InMonthly = table.Column<bool>(type: "bit", nullable: false),
                    WindowResultsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyTuningResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StrategyTuningRuns",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StrategyKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TargetCurrency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    PrimaryTimeFrame = table.Column<int>(type: "int", nullable: false),
                    SubCandleTimeFrame = table.Column<int>(type: "int", nullable: true),
                    DateFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InitialCapital = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SlippagePercent = table.Column<decimal>(type: "decimal(6,4)", precision: 6, scale: 4, nullable: false),
                    SelectedScales = table.Column<int>(type: "int", nullable: false),
                    PrimaryScale = table.Column<int>(type: "int", nullable: false),
                    MinTradesPerWindow = table.Column<int>(type: "int", nullable: false),
                    Algorithm = table.Column<int>(type: "int", nullable: false),
                    PopulationSize = table.Column<int>(type: "int", nullable: false),
                    Generations = table.Column<int>(type: "int", nullable: false),
                    MutationRate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    CrossoverRate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalEvaluations = table.Column<int>(type: "int", nullable: false),
                    CompletedEvaluations = table.Column<int>(type: "int", nullable: false),
                    BestResultIdYearly = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BestResultIdSemi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BestResultIdQuarterly = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BestResultIdMonthly = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyTuningRuns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StrategyTuningResults_Generation",
                schema: "trader",
                table: "StrategyTuningResults",
                column: "Generation");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyTuningResults_TuningRunId_AvgReturnCompoundYearly",
                schema: "trader",
                table: "StrategyTuningResults",
                columns: new[] { "TuningRunId", "AvgReturnCompoundYearly" });

            migrationBuilder.CreateIndex(
                name: "IX_StrategyTuningResults_TuningRunId_IsTop10InYearly_IsTop10InSemi_IsTop10InQuarterly_IsTop10InMonthly",
                schema: "trader",
                table: "StrategyTuningResults",
                columns: new[] { "TuningRunId", "IsTop10InYearly", "IsTop10InSemi", "IsTop10InQuarterly", "IsTop10InMonthly" });

            migrationBuilder.CreateIndex(
                name: "IX_StrategyTuningRuns_CreatedAt",
                schema: "trader",
                table: "StrategyTuningRuns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyTuningRuns_Isin_StrategyKey_Status",
                schema: "trader",
                table: "StrategyTuningRuns",
                columns: new[] { "Isin", "StrategyKey", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StrategyTuningResults",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "StrategyTuningRuns",
                schema: "trader");
        }
    }
}
