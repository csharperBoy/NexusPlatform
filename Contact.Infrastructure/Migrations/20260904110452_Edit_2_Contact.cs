using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contact.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_2_Contact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactResources_ContactResources_ParentContactResourceId",
                schema: "contact",
                table: "ContactResources");

            migrationBuilder.RenameColumn(
                name: "ParentContactResourceId",
                schema: "contact",
                table: "ContactResources",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_ContactResources_ParentContactResourceId",
                schema: "contact",
                table: "ContactResources",
                newName: "IX_ContactResources_ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactResources_ContactResources_ParentId",
                schema: "contact",
                table: "ContactResources",
                column: "ParentId",
                principalSchema: "contact",
                principalTable: "ContactResources",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactResources_ContactResources_ParentId",
                schema: "contact",
                table: "ContactResources");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                schema: "contact",
                table: "ContactResources",
                newName: "ParentContactResourceId");

            migrationBuilder.RenameIndex(
                name: "IX_ContactResources_ParentId",
                schema: "contact",
                table: "ContactResources",
                newName: "IX_ContactResources_ParentContactResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactResources_ContactResources_ParentContactResourceId",
                schema: "contact",
                table: "ContactResources",
                column: "ParentContactResourceId",
                principalSchema: "contact",
                principalTable: "ContactResources",
                principalColumn: "Id");
        }
    }
}
