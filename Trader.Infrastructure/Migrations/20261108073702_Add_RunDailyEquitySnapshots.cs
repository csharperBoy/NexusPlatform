using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_RunDailyEquitySnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RunDailyEquitySnapshots",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_RunDailyEquitySnapshot", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RunDailyEquitySnapshot_CreatedAt",
                schema: "trader",
                table: "RunDailyEquitySnapshots",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RunDailyEquitySnapshot_CreatedBy",
                schema: "trader",
                table: "RunDailyEquitySnapshots",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RunDailyEquitySnapshot_Date",
                schema: "trader",
                table: "RunDailyEquitySnapshots",
                column: "TradingDate");

            migrationBuilder.CreateIndex(
                name: "IX_RunDailyEquitySnapshot_ModifiedAt",
                schema: "trader",
                table: "RunDailyEquitySnapshots",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RunDailyEquitySnapshot_ModifiedBy",
                schema: "trader",
                table: "RunDailyEquitySnapshots",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RunDailyEquitySnapshots_Id",
                schema: "trader",
                table: "RunDailyEquitySnapshots",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_RunDailyEquitySnapshot_Run_Date",
                schema: "trader",
                table: "RunDailyEquitySnapshots",
                columns: new[] { "RunId", "TradingDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RunDailyEquitySnapshots",
                schema: "trader");
        }
    }
}
