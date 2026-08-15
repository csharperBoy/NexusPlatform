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
            migrationBuilder.RenameTable(
                name: "PostContacts",
                schema: "hr",
                newName: "PostContacts",
                newSchema: "contact");

            migrationBuilder.RenameTable(
                name: "PartyContacts",
                schema: "people",
                newName: "PartyContacts",
                newSchema: "contact");

            migrationBuilder.RenameTable(
                name: "LocationContacts",
                schema: "hr",
                newName: "LocationContacts",
                newSchema: "contact");

            migrationBuilder.RenameTable(
                name: "EmploymentContacts",
                schema: "hr",
                newName: "EmploymentContacts",
                newSchema: "contact");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hr");

            migrationBuilder.EnsureSchema(
                name: "people");

            migrationBuilder.RenameTable(
                name: "PostContacts",
                schema: "contact",
                newName: "PostContacts",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: "PartyContacts",
                schema: "contact",
                newName: "PartyContacts",
                newSchema: "people");

            migrationBuilder.RenameTable(
                name: "LocationContacts",
                schema: "contact",
                newName: "LocationContacts",
                newSchema: "hr");

            migrationBuilder.RenameTable(
                name: "EmploymentContacts",
                schema: "contact",
                newName: "EmploymentContacts",
                newSchema: "hr");
        }
    }
}
