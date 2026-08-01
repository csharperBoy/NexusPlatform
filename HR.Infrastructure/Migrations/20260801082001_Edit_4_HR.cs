using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_4_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "hr",
                table: " Employment",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "hr",
                table: " Employment",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                schema: "hr",
                table: " Employment",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "hr",
                table: " Employment",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employment_CreatedAt",
                schema: "hr",
                table: " Employment",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Employment_CreatedBy",
                schema: "hr",
                table: " Employment",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Employment_ModifiedAt",
                schema: "hr",
                table: " Employment",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Employment_ModifiedBy",
                schema: "hr",
                table: " Employment",
                column: "ModifiedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employment_CreatedAt",
                schema: "hr",
                table: " Employment");

            migrationBuilder.DropIndex(
                name: "IX_Employment_CreatedBy",
                schema: "hr",
                table: " Employment");

            migrationBuilder.DropIndex(
                name: "IX_Employment_ModifiedAt",
                schema: "hr",
                table: " Employment");

            migrationBuilder.DropIndex(
                name: "IX_Employment_ModifiedBy",
                schema: "hr",
                table: " Employment");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "hr",
                table: " Employment");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "hr",
                table: " Employment");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "hr",
                table: " Employment");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "hr",
                table: " Employment");
        }
    }
}
