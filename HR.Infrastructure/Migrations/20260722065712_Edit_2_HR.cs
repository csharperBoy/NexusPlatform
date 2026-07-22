using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_2_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployementInfoViews",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "PostInfoViews",
                schema: "hr");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "hr",
                table: "JobTitle",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "hr",
                table: "JobTitle",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                schema: "hr",
                table: "JobTitle",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "hr",
                table: "JobTitle",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobTitle_CreatedAt",
                schema: "hr",
                table: "JobTitle",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobTitle_CreatedBy",
                schema: "hr",
                table: "JobTitle",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JobTitle_ModifiedAt",
                schema: "hr",
                table: "JobTitle",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobTitle_ModifiedBy",
                schema: "hr",
                table: "JobTitle",
                column: "ModifiedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobTitle_CreatedAt",
                schema: "hr",
                table: "JobTitle");

            migrationBuilder.DropIndex(
                name: "IX_JobTitle_CreatedBy",
                schema: "hr",
                table: "JobTitle");

            migrationBuilder.DropIndex(
                name: "IX_JobTitle_ModifiedAt",
                schema: "hr",
                table: "JobTitle");

            migrationBuilder.DropIndex(
                name: "IX_JobTitle_ModifiedBy",
                schema: "hr",
                table: "JobTitle");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "hr",
                table: "JobTitle");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "hr",
                table: "JobTitle");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "hr",
                table: "JobTitle");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "hr",
                table: "JobTitle");

            migrationBuilder.CreateTable(
                name: "EmployementInfoViews",
                schema: "hr",
                columns: table => new
                {
                    AssignmentsAssigneeType = table.Column<int>(type: "int", nullable: false),
                    AssignmentsEffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    AssignmentsEffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    CostCenterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeEffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EmployeeEffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    EmployeeStatusName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmploymentLocationsEffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EmploymentLocationsEffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GradeTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobLevelTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobTitleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganizationUnitsName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartyAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartyEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartyMobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartyPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostContactFax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostContactMobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "PostInfoViews",
                schema: "hr",
                columns: table => new
                {
                    AssignmentsAssigneeType = table.Column<int>(type: "int", nullable: false),
                    CostCenterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkCostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkGradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkJobLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkJobTitleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GradeTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobLevelTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobTitleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NationalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficePhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrgEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrgMobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationUnitsName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });
        }
    }
}
