using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_12_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemove",
                schema: "hr",
                table: "Post",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemove",
                schema: "hr",
                table: "Location",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemove",
                schema: "hr",
                table: "Employment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemove",
                schema: "hr",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "IsRemove",
                schema: "hr",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "IsRemove",
                schema: "hr",
                table: "Employment");
        }
    }
}
