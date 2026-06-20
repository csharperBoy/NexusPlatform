using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit1_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Post_CostCenter_CostCenterId",
                schema: "hr",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_Grade_GradeId",
                schema: "hr",
                table: "Post");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Grade",
                schema: "organization",
                table: "Grade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CostCenter",
                schema: "organization",
                table: "CostCenter");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                schema: "organization",
                newName: "OutboxMessages",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: "OrganizationUnits",
                schema: "organization",
                newName: "OrganizationUnits",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: "JobTitle",
                schema: "organization",
                newName: "JobTitle",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: "JobLevel",
                schema: "organization",
                newName: "JobLevel",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: "Grade",
                schema: "organization",
                newName: " Grade",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: "CostCenter",
                schema: "organization",
                newName: " CostCenter",
                newSchema: "hr");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ Grade",
                schema: "hr",
                table: " Grade",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ CostCenter",
                schema: "hr",
                table: " CostCenter",
                column: "Id");

            migrationBuilder.CreateTable(
                name: " Employment",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ Employment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: " EmploymentStatus",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ EmploymentStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: " EmploymentType",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ EmploymentType", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Post_ CostCenter_CostCenterId",
                schema: "hr",
                table: "Post",
                column: "CostCenterId",
                principalSchema: "hr",
                principalTable: " CostCenter",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_ Grade_GradeId",
                schema: "hr",
                table: "Post",
                column: "GradeId",
                principalSchema: "hr",
                principalTable: " Grade",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Post_ CostCenter_CostCenterId",
                schema: "hr",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_ Grade_GradeId",
                schema: "hr",
                table: "Post");

            migrationBuilder.DropTable(
                name: " Employment",
                schema: "hr");

            migrationBuilder.DropTable(
                name: " EmploymentStatus",
                schema: "hr");

            migrationBuilder.DropTable(
                name: " EmploymentType",
                schema: "hr");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ Grade",
                schema: "hr",
                table: " Grade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ CostCenter",
                schema: "hr",
                table: " CostCenter");

            migrationBuilder.EnsureSchema(
                name: "organization");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                schema: "hr",
                newName: "OutboxMessages",
                newSchema: "organization");

            migrationBuilder.RenameTable(
                name: "OrganizationUnits",
                schema: "hr",
                newName: "OrganizationUnits",
                newSchema: "organization");

            migrationBuilder.RenameTable(
                name: "JobTitle",
                schema: "hr",
                newName: "JobTitle",
                newSchema: "organization");

            migrationBuilder.RenameTable(
                name: "JobLevel",
                schema: "hr",
                newName: "JobLevel",
                newSchema: "organization");

            migrationBuilder.RenameTable(
                name: " Grade",
                schema: "hr",
                newName: "Grade",
                newSchema: "organization");

            migrationBuilder.RenameTable(
                name: " CostCenter",
                schema: "hr",
                newName: "CostCenter",
                newSchema: "organization");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Grade",
                schema: "organization",
                table: "Grade",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CostCenter",
                schema: "organization",
                table: "CostCenter",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_CostCenter_CostCenterId",
                schema: "hr",
                table: "Post",
                column: "CostCenterId",
                principalSchema: "organization",
                principalTable: "CostCenter",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Grade_GradeId",
                schema: "hr",
                table: "Post",
                column: "GradeId",
                principalSchema: "organization",
                principalTable: "Grade",
                principalColumn: "Id");
        }
    }
}
