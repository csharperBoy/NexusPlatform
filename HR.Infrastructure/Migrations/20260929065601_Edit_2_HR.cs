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
            migrationBuilder.AlterColumn<bool>(
                name: "IsRemove",
                schema: "hr",
                table: "Post",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRemove",
                schema: "hr",
                table: "Location",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRemove",
                schema: "hr",
                table: "Employment",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_Post_IsRemove",
                schema: "hr",
                table: "Post",
                column: "IsRemove");

            migrationBuilder.CreateIndex(
                name: "IX_Location_IsRemove",
                schema: "hr",
                table: "Location",
                column: "IsRemove");

            migrationBuilder.CreateIndex(
                name: "IX_Employment_IsRemove",
                schema: "hr",
                table: "Employment",
                column: "IsRemove");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Post_IsRemove",
                schema: "hr",
                table: "Post");

            migrationBuilder.DropIndex(
                name: "IX_Location_IsRemove",
                schema: "hr",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Employment_IsRemove",
                schema: "hr",
                table: "Employment");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRemove",
                schema: "hr",
                table: "Post",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRemove",
                schema: "hr",
                table: "Location",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRemove",
                schema: "hr",
                table: "Employment",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}
