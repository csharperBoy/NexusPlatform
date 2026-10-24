using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_TradingSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TradingScheduleId",
                schema: "trader",
                table: "Instruments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TradingSchedules",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreMarketStart = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    SessionStart = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    SessionEnd = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    AfternoonStart = table.Column<TimeOnly>(type: "time(0)", nullable: true),
                    AfternoonEnd = table.Column<TimeOnly>(type: "time(0)", nullable: true),
                    AfterHoursEnd = table.Column<TimeOnly>(type: "time(0)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingSchedule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_TradingScheduleId",
                schema: "trader",
                table: "Instruments",
                column: "TradingScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingSchedule_IsActive",
                schema: "trader",
                table: "TradingSchedules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TradingSchedules_Id",
                schema: "trader",
                table: "TradingSchedules",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TradingSchedule_Code",
                schema: "trader",
                table: "TradingSchedules",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Instruments_TradingSchedules_TradingScheduleId",
                schema: "trader",
                table: "Instruments",
                column: "TradingScheduleId",
                principalSchema: "trader",
                principalTable: "TradingSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instruments_TradingSchedules_TradingScheduleId",
                schema: "trader",
                table: "Instruments");

            migrationBuilder.DropTable(
                name: "TradingSchedules",
                schema: "trader");

            migrationBuilder.DropIndex(
                name: "IX_Instrument_TradingScheduleId",
                schema: "trader",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "TradingScheduleId",
                schema: "trader",
                table: "Instruments");
        }
    }
}
