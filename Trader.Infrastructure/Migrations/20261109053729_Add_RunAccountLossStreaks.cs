using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_RunAccountLossStreaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AllocationPercent",
                schema: "trader",
                table: "TradingRunAccounts",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 100m);

            migrationBuilder.CreateTable(
                name: "RunAccountDailyEquitySnapshots",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TradingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OpeningCash = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OpeningPositionValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OpeningEquity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunAccountDailyEquitySnapshot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RunAccountLossStreaks",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsecutiveLosses = table.Column<int>(type: "int", nullable: false),
                    LastLossAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastWinAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CircuitBrokenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CooldownExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunAccountLossStreak", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountDailyEquity_Account_Date",
                schema: "trader",
                table: "RunAccountDailyEquitySnapshots",
                columns: new[] { "AccountId", "TradingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountDailyEquitySnapshot_CreatedAt",
                schema: "trader",
                table: "RunAccountDailyEquitySnapshots",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountDailyEquitySnapshot_CreatedBy",
                schema: "trader",
                table: "RunAccountDailyEquitySnapshots",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountDailyEquitySnapshot_ModifiedAt",
                schema: "trader",
                table: "RunAccountDailyEquitySnapshots",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountDailyEquitySnapshot_ModifiedBy",
                schema: "trader",
                table: "RunAccountDailyEquitySnapshots",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountDailyEquitySnapshots_Id",
                schema: "trader",
                table: "RunAccountDailyEquitySnapshots",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_RunAccountDailyEquity_Run_Account_Date",
                schema: "trader",
                table: "RunAccountDailyEquitySnapshots",
                columns: new[] { "RunId", "AccountId", "TradingDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountLossStreak_Account",
                schema: "trader",
                table: "RunAccountLossStreaks",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountLossStreak_CircuitBroken",
                schema: "trader",
                table: "RunAccountLossStreaks",
                column: "CircuitBrokenAt",
                filter: "[CircuitBrokenAt] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountLossStreak_CreatedAt",
                schema: "trader",
                table: "RunAccountLossStreaks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountLossStreak_CreatedBy",
                schema: "trader",
                table: "RunAccountLossStreaks",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountLossStreak_ModifiedAt",
                schema: "trader",
                table: "RunAccountLossStreaks",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountLossStreak_ModifiedBy",
                schema: "trader",
                table: "RunAccountLossStreaks",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RunAccountLossStreaks_Id",
                schema: "trader",
                table: "RunAccountLossStreaks",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_RunAccountLossStreak_Run_Account",
                schema: "trader",
                table: "RunAccountLossStreaks",
                columns: new[] { "RunId", "AccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RunAccountDailyEquitySnapshots",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "RunAccountLossStreaks",
                schema: "trader");

            migrationBuilder.DropColumn(
                name: "AllocationPercent",
                schema: "trader",
                table: "TradingRunAccounts");
        }
    }
}
