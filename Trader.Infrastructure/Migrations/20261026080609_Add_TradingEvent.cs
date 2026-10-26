using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_TradingEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TradingEvents",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SignalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderIntentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BrokerOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingEvent", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TradingEvent_Errors_Time",
                schema: "trader",
                table: "TradingEvents",
                columns: new[] { "Severity", "OccurredAt" },
                filter: "[Severity] >= 3");

            migrationBuilder.CreateIndex(
                name: "IX_TradingEvent_Isin_Time",
                schema: "trader",
                table: "TradingEvents",
                columns: new[] { "Isin", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TradingEvent_OrderIntentId",
                schema: "trader",
                table: "TradingEvents",
                column: "OrderIntentId",
                filter: "[OrderIntentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TradingEvent_PositionId",
                schema: "trader",
                table: "TradingEvents",
                column: "PositionId",
                filter: "[PositionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TradingEvent_Run_Time",
                schema: "trader",
                table: "TradingEvents",
                columns: new[] { "RunId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TradingEvent_SignalId",
                schema: "trader",
                table: "TradingEvents",
                column: "SignalId",
                filter: "[SignalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TradingEvent_Stage_Sev_Time",
                schema: "trader",
                table: "TradingEvents",
                columns: new[] { "Stage", "Severity", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TradingEvent_Time",
                schema: "trader",
                table: "TradingEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_TradingEvents_Id",
                schema: "trader",
                table: "TradingEvents",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TradingEvents",
                schema: "trader");
        }
    }
}
