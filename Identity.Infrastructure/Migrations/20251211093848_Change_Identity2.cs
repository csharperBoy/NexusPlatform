using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Change_Identity2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                schema: "identity",
                table: "UserSessions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceInfo",
                schema: "identity",
                table: "UserSessions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "identity",
                table: "UserSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                schema: "identity",
                table: "UserSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "identity",
                table: "UserSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "identity",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                schema: "identity",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "identity",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "identity",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "identity",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "identity",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "identity",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "identity",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "identity",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                schema: "identity",
                table: "UserSessions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceInfo",
                schema: "identity",
                table: "UserSessions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "identity",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
