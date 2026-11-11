using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_RefactorHistoricalDataCollection_Trader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgressPercent",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "ErrorMessage",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                newName: "LastError");

            migrationBuilder.RenameIndex(
                name: "IX_HistoricalDataCollectionJob_Status",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                newName: "IX_HistoricalDataCollectionJob_State");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTo",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<decimal>(
                name: "BackfillProgressPercent",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRunAt",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastRunInserted",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastRunSkipped",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Mode",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RecurrencePatternJson",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SchedulerJobId",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SchedulerRecurringId",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StoppedAt",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalDataCollectionJob_Mode",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                column: "Mode");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalDataCollectionJob_SchedulerJobId",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                column: "SchedulerJobId",
                filter: "[SchedulerJobId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalDataCollectionJob_SchedulerRecurringId",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                column: "SchedulerRecurringId",
                filter: "[SchedulerRecurringId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HistoricalDataCollectionJob_Mode",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropIndex(
                name: "IX_HistoricalDataCollectionJob_SchedulerJobId",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropIndex(
                name: "IX_HistoricalDataCollectionJob_SchedulerRecurringId",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropColumn(
                name: "BackfillProgressPercent",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropColumn(
                name: "LastRunAt",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropColumn(
                name: "LastRunInserted",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropColumn(
                name: "LastRunSkipped",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropColumn(
                name: "Mode",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropColumn(
                name: "RecurrencePatternJson",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropColumn(
                name: "SchedulerJobId",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropColumn(
                name: "SchedulerRecurringId",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropColumn(
                name: "StoppedAt",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.RenameColumn(
                name: "State",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "LastError",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                newName: "ErrorMessage");

            migrationBuilder.RenameIndex(
                name: "IX_HistoricalDataCollectionJob_State",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                newName: "IX_HistoricalDataCollectionJob_Status");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTo",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProgressPercent",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
