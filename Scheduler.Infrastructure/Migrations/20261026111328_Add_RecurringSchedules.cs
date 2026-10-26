using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_RecurringSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecurringScheduleId",
                schema: "scheduler",
                table: "ScheduledJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecurringSchedules",
                schema: "scheduler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecurrencePatternJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MaxOccurrences = table.Column<int>(type: "int", nullable: true),
                    MaxFailures = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    State = table.Column<int>(type: "int", nullable: false),
                    OccurredCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FailureCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastRunAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NextRunAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CurrentScheduledJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringSchedule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJob_RecurringScheduleId",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "RecurringScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringSchedule_CreatedAt",
                schema: "scheduler",
                table: "RecurringSchedules",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringSchedule_CreatedBy",
                schema: "scheduler",
                table: "RecurringSchedules",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringSchedule_ModifiedAt",
                schema: "scheduler",
                table: "RecurringSchedules",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringSchedule_ModifiedBy",
                schema: "scheduler",
                table: "RecurringSchedules",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringSchedule_OwnerOrgUnit",
                schema: "scheduler",
                table: "RecurringSchedules",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringSchedule_OwnerPerson",
                schema: "scheduler",
                table: "RecurringSchedules",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringSchedule_ScopedLookup",
                schema: "scheduler",
                table: "RecurringSchedules",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_RecurringSchedule_State",
                schema: "scheduler",
                table: "RecurringSchedules",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringSchedule_State_NextRun",
                schema: "scheduler",
                table: "RecurringSchedules",
                columns: new[] { "State", "NextRunAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RecurringSchedules_Id",
                schema: "scheduler",
                table: "RecurringSchedules",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_RecurringSchedule_Name",
                schema: "scheduler",
                table: "RecurringSchedules",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecurringSchedules",
                schema: "scheduler");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledJob_RecurringScheduleId",
                schema: "scheduler",
                table: "ScheduledJobs");

            migrationBuilder.DropColumn(
                name: "RecurringScheduleId",
                schema: "scheduler",
                table: "ScheduledJobs");
        }
    }
}
