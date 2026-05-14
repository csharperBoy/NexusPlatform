using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_4_Authorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JoinDetails",
                schema: "authorization");

            migrationBuilder.DropColumn(
                name: "JoinDetailId",
                schema: "authorization",
                table: "PermissionRules");

            migrationBuilder.AddColumn<string>(
                name: "JoinEntity",
                schema: "authorization",
                table: "PermissionRules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JoinForeignKey",
                schema: "authorization",
                table: "PermissionRules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JoinLocalKey",
                schema: "authorization",
                table: "PermissionRules",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JoinEntity",
                schema: "authorization",
                table: "PermissionRules");

            migrationBuilder.DropColumn(
                name: "JoinForeignKey",
                schema: "authorization",
                table: "PermissionRules");

            migrationBuilder.DropColumn(
                name: "JoinLocalKey",
                schema: "authorization",
                table: "PermissionRules");

            migrationBuilder.AddColumn<Guid>(
                name: "JoinDetailId",
                schema: "authorization",
                table: "PermissionRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JoinDetails",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    JoinEntity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinForeignKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinLocalKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JoinDetails_PermissionRules_PermissionRuleId",
                        column: x => x.PermissionRuleId,
                        principalSchema: "authorization",
                        principalTable: "PermissionRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JoinDetail_CreatedAt",
                schema: "authorization",
                table: "JoinDetails",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JoinDetail_CreatedBy",
                schema: "authorization",
                table: "JoinDetails",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JoinDetail_ModifiedAt",
                schema: "authorization",
                table: "JoinDetails",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JoinDetail_ModifiedBy",
                schema: "authorization",
                table: "JoinDetails",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JoinDetails_Id",
                schema: "authorization",
                table: "JoinDetails",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinDetails_PermissionRuleId",
                schema: "authorization",
                table: "JoinDetails",
                column: "PermissionRuleId",
                unique: true);
        }
    }
}
