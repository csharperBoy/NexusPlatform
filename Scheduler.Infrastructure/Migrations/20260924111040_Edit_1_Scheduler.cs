using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_1_Scheduler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scheduler");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "scheduler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AssemblyQualifiedName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ErrorStackTrace = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    EventVersion = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledJobs",
                schema: "scheduler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    JobType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HangfireJobId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FireAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PreFireBuffer = table.Column<long>(type: "bigint", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledJob", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                schema: "scheduler",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_OccurredOnUtc",
                schema: "scheduler",
                table: "OutboxMessages",
                columns: new[] { "Status", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_TypeName",
                schema: "scheduler",
                table: "OutboxMessages",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJob_CreatedAt",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJob_CreatedBy",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJob_ModifiedAt",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJob_ModifiedBy",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJob_OwnerOrgUnit",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJob_OwnerPerson",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJob_ScopedLookup",
                schema: "scheduler",
                table: "ScheduledJobs",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_FireAt",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "FireAt");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_HangfireJobId",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "HangfireJobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_Id",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledJobs_State",
                schema: "scheduler",
                table: "ScheduledJobs",
                column: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "scheduler");

            migrationBuilder.DropTable(
                name: "ScheduledJobs",
                schema: "scheduler");
        }
    }
}
