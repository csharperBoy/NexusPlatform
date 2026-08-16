using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_10_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: " Grade",
                schema: "hr",
                newName: "Grade",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: " EmploymentType",
                schema: "hr",
                newName: "EmploymentType",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: " EmploymentStatus",
                schema: "hr",
                newName: "EmploymentStatus",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: " EmploymentLocations",
                schema: "hr",
                newName: "EmploymentLocations",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: " Employment",
                schema: "hr",
                newName: "Employment",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: " CostCenter",
                schema: "hr",
                newName: "CostCenter",
                newSchema: "hr");

            migrationBuilder.RenameIndex(
                name: "IX_ Grade_Id",
                schema: "hr",
                table: "Grade",
                newName: "IX_Grade_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ EmploymentType_Id",
                schema: "hr",
                table: "EmploymentType",
                newName: "IX_EmploymentType_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ EmploymentStatus_Id",
                schema: "hr",
                table: "EmploymentStatus",
                newName: "IX_EmploymentStatus_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ EmploymentLocations_Id",
                schema: "hr",
                table: "EmploymentLocations",
                newName: "IX_EmploymentLocations_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ Employment_Id",
                schema: "hr",
                table: "Employment",
                newName: "IX_Employment_Id");

            migrationBuilder.RenameIndex(
                name: "IX_ Employment_FkEmploymentTypeId",
                schema: "hr",
                table: "Employment",
                newName: "IX_Employment_FkEmploymentTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ Employment_FkEmploymentStatusId",
                schema: "hr",
                table: "Employment",
                newName: "IX_Employment_FkEmploymentStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_ CostCenter_Id",
                schema: "hr",
                table: "CostCenter",
                newName: "IX_CostCenter_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Grade",
                schema: "hr",
                newName: " Grade",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: "EmploymentType",
                schema: "hr",
                newName: " EmploymentType",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: "EmploymentStatus",
                schema: "hr",
                newName: " EmploymentStatus",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: "EmploymentLocations",
                schema: "hr",
                newName: " EmploymentLocations",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: "Employment",
                schema: "hr",
                newName: " Employment",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: "CostCenter",
                schema: "hr",
                newName: " CostCenter",
                newSchema: "hr");

            migrationBuilder.RenameIndex(
                name: "IX_Grade_Id",
                schema: "hr",
                table: " Grade",
                newName: "IX_ Grade_Id");

            migrationBuilder.RenameIndex(
                name: "IX_EmploymentType_Id",
                schema: "hr",
                table: " EmploymentType",
                newName: "IX_ EmploymentType_Id");

            migrationBuilder.RenameIndex(
                name: "IX_EmploymentStatus_Id",
                schema: "hr",
                table: " EmploymentStatus",
                newName: "IX_ EmploymentStatus_Id");

            migrationBuilder.RenameIndex(
                name: "IX_EmploymentLocations_Id",
                schema: "hr",
                table: " EmploymentLocations",
                newName: "IX_ EmploymentLocations_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Employment_Id",
                schema: "hr",
                table: " Employment",
                newName: "IX_ Employment_Id");

            migrationBuilder.RenameIndex(
                name: "IX_Employment_FkEmploymentTypeId",
                schema: "hr",
                table: " Employment",
                newName: "IX_ Employment_FkEmploymentTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Employment_FkEmploymentStatusId",
                schema: "hr",
                table: " Employment",
                newName: "IX_ Employment_FkEmploymentStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_CostCenter_Id",
                schema: "hr",
                table: " CostCenter",
                newName: "IX_ CostCenter_Id");
        }
    }
}
