using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_2_Authorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PermissionRule_Permissions_PermissionId",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Scope_ResourceId",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Scope_Scope_ParentId",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Scope",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PermissionRule",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JoinDetail",
                schema: "authorization",
                table: "JoinDetail");

            migrationBuilder.RenameTable(
                name: "Scope",
                schema: "authorization",
                newName: "Resources",
                newSchema: "authorization");

            migrationBuilder.RenameTable(
                name: "PermissionRule",
                schema: "authorization",
                newName: "PermissionRules",
                newSchema: "authorization");

            migrationBuilder.RenameTable(
                name: "JoinDetail",
                schema: "authorization",
                newName: "JoinDetails",
                newSchema: "authorization");

            migrationBuilder.RenameIndex(
                name: "IX_Scope_ResourcePath",
                schema: "authorization",
                table: "Resources",
                newName: "IX_Resources_ResourcePath");

            migrationBuilder.RenameIndex(
                name: "IX_Scope_ParentId",
                schema: "authorization",
                table: "Resources",
                newName: "IX_Resources_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_Scope_Key",
                schema: "authorization",
                table: "Resources",
                newName: "IX_Resources_Key");

            migrationBuilder.RenameIndex(
                name: "IX_PermissionRule_PermissionId",
                schema: "authorization",
                table: "PermissionRules",
                newName: "IX_PermissionRules_PermissionId");

            migrationBuilder.RenameIndex(
                name: "IX_PermissionRule_Id",
                schema: "authorization",
                table: "PermissionRules",
                newName: "IX_PermissionRules_Id");

            migrationBuilder.RenameIndex(
                name: "IX_JoinDetail_Id",
                schema: "authorization",
                table: "JoinDetails",
                newName: "IX_JoinDetails_Id");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                schema: "authorization",
                table: "Scopes",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "authorization",
                table: "Scopes",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "authorization",
                table: "Scopes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Resources",
                schema: "authorization",
                table: "Resources",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PermissionRules",
                schema: "authorization",
                table: "PermissionRules",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JoinDetails",
                schema: "authorization",
                table: "JoinDetails",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Scope_CreatedAt",
                schema: "authorization",
                table: "Scopes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Scope_CreatedBy",
                schema: "authorization",
                table: "Scopes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Scope_ModifiedAt",
                schema: "authorization",
                table: "Scopes",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Scope_ModifiedBy",
                schema: "authorization",
                table: "Scopes",
                column: "ModifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionRules_Permissions_PermissionId",
                schema: "authorization",
                table: "PermissionRules",
                column: "PermissionId",
                principalSchema: "authorization",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Resources_ResourceId",
                schema: "authorization",
                table: "Permissions",
                column: "ResourceId",
                principalSchema: "authorization",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Resources_ParentId",
                schema: "authorization",
                table: "Resources",
                column: "ParentId",
                principalSchema: "authorization",
                principalTable: "Resources",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PermissionRules_Permissions_PermissionId",
                schema: "authorization",
                table: "PermissionRules");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Resources_ResourceId",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Resources_ParentId",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Scope_CreatedAt",
                schema: "authorization",
                table: "Scopes");

            migrationBuilder.DropIndex(
                name: "IX_Scope_CreatedBy",
                schema: "authorization",
                table: "Scopes");

            migrationBuilder.DropIndex(
                name: "IX_Scope_ModifiedAt",
                schema: "authorization",
                table: "Scopes");

            migrationBuilder.DropIndex(
                name: "IX_Scope_ModifiedBy",
                schema: "authorization",
                table: "Scopes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Resources",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PermissionRules",
                schema: "authorization",
                table: "PermissionRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JoinDetails",
                schema: "authorization",
                table: "JoinDetails");

            migrationBuilder.RenameTable(
                name: "Resources",
                schema: "authorization",
                newName: "Scope",
                newSchema: "authorization");

            migrationBuilder.RenameTable(
                name: "PermissionRules",
                schema: "authorization",
                newName: "PermissionRule",
                newSchema: "authorization");

            migrationBuilder.RenameTable(
                name: "JoinDetails",
                schema: "authorization",
                newName: "JoinDetail",
                newSchema: "authorization");

            migrationBuilder.RenameIndex(
                name: "IX_Resources_ResourcePath",
                schema: "authorization",
                table: "Scope",
                newName: "IX_Scope_ResourcePath");

            migrationBuilder.RenameIndex(
                name: "IX_Resources_ParentId",
                schema: "authorization",
                table: "Scope",
                newName: "IX_Scope_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_Resources_Key",
                schema: "authorization",
                table: "Scope",
                newName: "IX_Scope_Key");

            migrationBuilder.RenameIndex(
                name: "IX_PermissionRules_PermissionId",
                schema: "authorization",
                table: "PermissionRule",
                newName: "IX_PermissionRule_PermissionId");

            migrationBuilder.RenameIndex(
                name: "IX_PermissionRules_Id",
                schema: "authorization",
                table: "PermissionRule",
                newName: "IX_PermissionRule_Id");

            migrationBuilder.RenameIndex(
                name: "IX_JoinDetails_Id",
                schema: "authorization",
                table: "JoinDetail",
                newName: "IX_JoinDetail_Id");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                schema: "authorization",
                table: "Scopes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "authorization",
                table: "Scopes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "authorization",
                table: "Scopes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Scope",
                schema: "authorization",
                table: "Scope",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PermissionRule",
                schema: "authorization",
                table: "PermissionRule",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JoinDetail",
                schema: "authorization",
                table: "JoinDetail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionRule_Permissions_PermissionId",
                schema: "authorization",
                table: "PermissionRule",
                column: "PermissionId",
                principalSchema: "authorization",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Scope_ResourceId",
                schema: "authorization",
                table: "Permissions",
                column: "ResourceId",
                principalSchema: "authorization",
                principalTable: "Scope",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Scope_Scope_ParentId",
                schema: "authorization",
                table: "Scope",
                column: "ParentId",
                principalSchema: "authorization",
                principalTable: "Scope",
                principalColumn: "Id");
        }
    }
}
