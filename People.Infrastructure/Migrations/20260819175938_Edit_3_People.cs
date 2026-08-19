using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace People.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_3_People : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FkContactProfileId",
                schema: "people",
                table: "Parties",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FkContactProfileId",
                schema: "people",
                table: "Parties");
        }
    }
}
