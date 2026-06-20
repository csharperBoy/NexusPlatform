using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "HR");

            migrationBuilder.EnsureSchema(
                name: "organization");

            migrationBuilder.EnsureSchema(
                name: "hr");

            migrationBuilder.CreateTable(
                name: "CostCenter",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grade",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobLevel",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobLevel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobTitle",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTitle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnits",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Path = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnits_OrganizationUnits_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "organization",
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "organization",
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
                name: "Post",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobTitleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReportsToPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ReportsToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Post_CostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "organization",
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_Grade_GradeId",
                        column: x => x.GradeId,
                        principalSchema: "organization",
                        principalTable: "Grade",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_JobLevel_JobLevelId",
                        column: x => x.JobLevelId,
                        principalSchema: "organization",
                        principalTable: "JobLevel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_JobTitle_JobTitleId",
                        column: x => x.JobTitleId,
                        principalSchema: "organization",
                        principalTable: "JobTitle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Post_OrganizationUnits_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "organization",
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Post_Post_ReportsToId",
                        column: x => x.ReportsToId,
                        principalSchema: "hr",
                        principalTable: "Post",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                schema: "HR",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssigneeType = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_Post_PostId",
                        column: x => x.PostId,
                        principalSchema: "hr",
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_CreatedAt",
                schema: "HR",
                table: "Assignments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_CreatedBy",
                schema: "HR",
                table: "Assignments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_ModifiedAt",
                schema: "HR",
                table: "Assignments",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_ModifiedBy",
                schema: "HR",
                table: "Assignments",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_Id",
                schema: "HR",
                table: "Assignments",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_PostId",
                schema: "HR",
                table: "Assignments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_CreatedAt",
                schema: "organization",
                table: "OrganizationUnits",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_CreatedBy",
                schema: "organization",
                table: "OrganizationUnits",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_ModifiedAt",
                schema: "organization",
                table: "OrganizationUnits",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_ModifiedBy",
                schema: "organization",
                table: "OrganizationUnits",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_Code",
                schema: "organization",
                table: "OrganizationUnits",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_ParentId",
                schema: "organization",
                table: "OrganizationUnits",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_Path",
                schema: "organization",
                table: "OrganizationUnits",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                schema: "organization",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_OccurredOnUtc",
                schema: "organization",
                table: "OutboxMessages",
                columns: new[] { "Status", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_TypeName",
                schema: "organization",
                table: "OutboxMessages",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_Post_CostCenterId",
                schema: "hr",
                table: "Post",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_CreatedAt",
                schema: "hr",
                table: "Post",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Post_CreatedBy",
                schema: "hr",
                table: "Post",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Post_GradeId",
                schema: "hr",
                table: "Post",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_JobLevelId",
                schema: "hr",
                table: "Post",
                column: "JobLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_JobTitleId",
                schema: "hr",
                table: "Post",
                column: "JobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_ModifiedAt",
                schema: "hr",
                table: "Post",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Post_ModifiedBy",
                schema: "hr",
                table: "Post",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Post_OrganizationUnitId",
                schema: "hr",
                table: "Post",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_ReportsToId",
                schema: "hr",
                table: "Post",
                column: "ReportsToId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignments",
                schema: "HR");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "Post",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "CostCenter",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "Grade",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "JobLevel",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "JobTitle",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "OrganizationUnits",
                schema: "organization");
        }
    }
}
