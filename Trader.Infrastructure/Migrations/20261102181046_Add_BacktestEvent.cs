using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_BacktestEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BacktestEvents",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BacktestRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CandleTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacktestEvent", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BacktestEvent_Run_Severity",
                schema: "trader",
                table: "BacktestEvents",
                columns: new[] { "BacktestRunId", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_BacktestEvent_Run_Time",
                schema: "trader",
                table: "BacktestEvents",
                columns: new[] { "BacktestRunId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BacktestEvent_Run_Type",
                schema: "trader",
                table: "BacktestEvents",
                columns: new[] { "BacktestRunId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_BacktestEvents_Id",
                schema: "trader",
                table: "BacktestEvents",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BacktestEvents",
                schema: "trader");
        }
    }
}
