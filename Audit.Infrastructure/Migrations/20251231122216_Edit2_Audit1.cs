using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit2_Audit1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "audit",
                table: "AuditLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "audit",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                schema: "audit",
                table: "AuditLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "audit",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerOrganizationUnitId",
                schema: "audit",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerPersonId",
                schema: "audit",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerPositionId",
                schema: "audit",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "audit",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "audit",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "audit",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "audit",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OwnerOrganizationUnitId",
                schema: "audit",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OwnerPersonId",
                schema: "audit",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OwnerPositionId",
                schema: "audit",
                table: "AuditLogs");
        }
    }
}
