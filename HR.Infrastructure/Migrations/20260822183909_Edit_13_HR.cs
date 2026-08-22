using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_13_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_ Employment",
                schema: "hr",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ Employment_ EmploymentStatus",
                schema: "hr",
                table: "Employment");

            migrationBuilder.DropForeignKey(
                name: "FK_ Employment_ EmploymentType",
                schema: "hr",
                table: "Employment");

            migrationBuilder.DropForeignKey(
                name: "FK_ EmploymentLocations_ Employment",
                schema: "hr",
                table: "EmploymentLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ EmploymentLocations_Location",
                schema: "hr",
                table: "EmploymentLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_ CostCenter",
                schema: "hr",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_ Grade",
                schema: "hr",
                table: "Post");

            migrationBuilder.RenameIndex(
                name: "IX_ EmploymentLocations_fkLocationId",
                schema: "hr",
                table: "EmploymentLocations",
                newName: "IX_EmploymentLocations_fkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_ EmploymentLocations_fkEmploymentId",
                schema: "hr",
                table: "EmploymentLocations",
                newName: "IX_EmploymentLocations_fkEmploymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Employment",
                schema: "hr",
                table: "Assignments",
                column: "FkEmploymentId",
                principalSchema: "hr",
                principalTable: "Employment",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employment_EmploymentStatus",
                schema: "hr",
                table: "Employment",
                column: "FkEmploymentStatusId",
                principalSchema: "hr",
                principalTable: "EmploymentStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Employment_EmploymentType",
                schema: "hr",
                table: "Employment",
                column: "FkEmploymentTypeId",
                principalSchema: "hr",
                principalTable: "EmploymentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EmploymentLocations_Employment",
                schema: "hr",
                table: "EmploymentLocations",
                column: "FkEmploymentId",
                principalSchema: "hr",
                principalTable: "Employment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmploymentLocations_Location",
                schema: "hr",
                table: "EmploymentLocations",
                column: "FkLocationId",
                principalSchema: "hr",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Post_CostCenter",
                schema: "hr",
                table: "Post",
                column: "FkCostCenterId",
                principalSchema: "hr",
                principalTable: "CostCenter",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_Grade",
                schema: "hr",
                table: "Post",
                column: "FkGradeId",
                principalSchema: "hr",
                principalTable: "Grade",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Employment",
                schema: "hr",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Employment_EmploymentStatus",
                schema: "hr",
                table: "Employment");

            migrationBuilder.DropForeignKey(
                name: "FK_Employment_EmploymentType",
                schema: "hr",
                table: "Employment");

            migrationBuilder.DropForeignKey(
                name: "FK_EmploymentLocations_Employment",
                schema: "hr",
                table: "EmploymentLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_EmploymentLocations_Location",
                schema: "hr",
                table: "EmploymentLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_CostCenter",
                schema: "hr",
                table: "Post");

            migrationBuilder.DropForeignKey(
                name: "FK_Post_Grade",
                schema: "hr",
                table: "Post");

            migrationBuilder.RenameIndex(
                name: "IX_EmploymentLocations_fkLocationId",
                schema: "hr",
                table: "EmploymentLocations",
                newName: "IX_ EmploymentLocations_fkLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_EmploymentLocations_fkEmploymentId",
                schema: "hr",
                table: "EmploymentLocations",
                newName: "IX_ EmploymentLocations_fkEmploymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_ Employment",
                schema: "hr",
                table: "Assignments",
                column: "FkEmploymentId",
                principalSchema: "hr",
                principalTable: "Employment",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ Employment_ EmploymentStatus",
                schema: "hr",
                table: "Employment",
                column: "FkEmploymentStatusId",
                principalSchema: "hr",
                principalTable: "EmploymentStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ Employment_ EmploymentType",
                schema: "hr",
                table: "Employment",
                column: "FkEmploymentTypeId",
                principalSchema: "hr",
                principalTable: "EmploymentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ EmploymentLocations_ Employment",
                schema: "hr",
                table: "EmploymentLocations",
                column: "FkEmploymentId",
                principalSchema: "hr",
                principalTable: "Employment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ EmploymentLocations_Location",
                schema: "hr",
                table: "EmploymentLocations",
                column: "FkLocationId",
                principalSchema: "hr",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Post_ CostCenter",
                schema: "hr",
                table: "Post",
                column: "FkCostCenterId",
                principalSchema: "hr",
                principalTable: "CostCenter",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Post_ Grade",
                schema: "hr",
                table: "Post",
                column: "FkGradeId",
                principalSchema: "hr",
                principalTable: "Grade",
                principalColumn: "Id");
        }
    }
}
