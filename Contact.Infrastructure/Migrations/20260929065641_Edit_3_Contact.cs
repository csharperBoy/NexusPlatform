using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contact.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_3_Contact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemove",
                schema: "contact",
                table: "ContactResources",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ContactResource_IsRemove",
                schema: "contact",
                table: "ContactResources",
                column: "IsRemove");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContactResource_IsRemove",
                schema: "contact",
                table: "ContactResources");

            migrationBuilder.DropColumn(
                name: "IsRemove",
                schema: "contact",
                table: "ContactResources");
        }
    }
}
