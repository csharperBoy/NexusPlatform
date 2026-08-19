using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_11_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FkContactProfileId",
                schema: "hr",
                table: "Post",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FkContactProfileId",
                schema: "hr",
                table: "Location",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FkContactProfileId",
                schema: "hr",
                table: "Employment",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FkContactProfileId",
                schema: "hr",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "FkContactProfileId",
                schema: "hr",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "FkContactProfileId",
                schema: "hr",
                table: "Employment");
        }
    }
}
