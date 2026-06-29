using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit5_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Assignments_EmploymentId",
                schema: "HR",
                table: "Assignments",
                column: "EmploymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_ Employment_EmploymentId",
                schema: "HR",
                table: "Assignments",
                column: "EmploymentId",
                principalSchema: "hr",
                principalTable: " Employment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_ Employment_EmploymentId",
                schema: "HR",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_EmploymentId",
                schema: "HR",
                table: "Assignments");
        }
    }
}
