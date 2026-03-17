using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_1_Authorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Resources_ResourceId",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Scope_Permissions_PermissionId",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropTable(
                name: "Resources",
                schema: "authorization");

            migrationBuilder.DropIndex(
                name: "IX_Scope_PermissionId",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "PermissionId",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "JoinEntity",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.DropColumn(
                name: "JoinField",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.DropColumn(
                name: "JoinForeignKey",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.DropColumn(
                name: "JoinLocalKey",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.RenameColumn(
                name: "scope",
                schema: "authorization",
                table: "Scope",
                newName: "Type");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                schema: "authorization",
                table: "Scope",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "authorization",
                table: "Scope",
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
                table: "Scope",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<byte>(
                name: "Category",
                schema: "authorization",
                table: "Scope",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "authorization",
                table: "Scope",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                schema: "authorization",
                table: "Scope",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                schema: "authorization",
                table: "Scope",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "authorization",
                table: "Scope",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Key",
                schema: "authorization",
                table: "Scope",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "authorization",
                table: "Scope",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerOrganizationUnitId",
                schema: "authorization",
                table: "Scope",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerPersonId",
                schema: "authorization",
                table: "Scope",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerPositionId",
                schema: "authorization",
                table: "Scope",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                schema: "authorization",
                table: "Scope",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                schema: "authorization",
                table: "Scope",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResourcePath",
                schema: "authorization",
                table: "Scope",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "hasFieldBaseCondition",
                schema: "authorization",
                table: "Scope",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "hasRelationBaseCondition",
                schema: "authorization",
                table: "Scope",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "hasScope",
                schema: "authorization",
                table: "Scope",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                schema: "authorization",
                table: "PermissionRule",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "authorization",
                table: "PermissionRule",
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
                table: "PermissionRule",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "JoinDetailId",
                schema: "authorization",
                table: "PermissionRule",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JoinDetail",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PermissionRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoinLocalKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinForeignKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinEntity = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scopes",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    scope = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scopes_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "authorization",
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Resource_CreatedAt",
                schema: "authorization",
                table: "Scope",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_CreatedBy",
                schema: "authorization",
                table: "Scope",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_ModifiedAt",
                schema: "authorization",
                table: "Scope",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_ModifiedBy",
                schema: "authorization",
                table: "Scope",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_OwnerOrgUnit",
                schema: "authorization",
                table: "Scope",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_OwnerPerson",
                schema: "authorization",
                table: "Scope",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_ScopedLookup",
                schema: "authorization",
                table: "Scope",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_Scope_Key",
                schema: "authorization",
                table: "Scope",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scope_ParentId",
                schema: "authorization",
                table: "Scope",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Scope_ResourcePath",
                schema: "authorization",
                table: "Scope",
                column: "ResourcePath");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRule_CreatedAt",
                schema: "authorization",
                table: "PermissionRule",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRule_CreatedBy",
                schema: "authorization",
                table: "PermissionRule",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRule_Id",
                schema: "authorization",
                table: "PermissionRule",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRule_ModifiedAt",
                schema: "authorization",
                table: "PermissionRule",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRule_ModifiedBy",
                schema: "authorization",
                table: "PermissionRule",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JoinDetail_CreatedAt",
                schema: "authorization",
                table: "JoinDetail",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JoinDetail_CreatedBy",
                schema: "authorization",
                table: "JoinDetail",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JoinDetail_Id",
                schema: "authorization",
                table: "JoinDetail",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JoinDetail_ModifiedAt",
                schema: "authorization",
                table: "JoinDetail",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JoinDetail_ModifiedBy",
                schema: "authorization",
                table: "JoinDetail",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Scopes_PermissionId",
                schema: "authorization",
                table: "Scopes",
                column: "PermissionId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Scope_ResourceId",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Scope_Scope_ParentId",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropTable(
                name: "JoinDetail",
                schema: "authorization");

            migrationBuilder.DropTable(
                name: "Scopes",
                schema: "authorization");

            migrationBuilder.DropIndex(
                name: "IX_Resource_CreatedAt",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropIndex(
                name: "IX_Resource_CreatedBy",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropIndex(
                name: "IX_Resource_ModifiedAt",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropIndex(
                name: "IX_Resource_ModifiedBy",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropIndex(
                name: "IX_Resource_OwnerOrgUnit",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropIndex(
                name: "IX_Resource_OwnerPerson",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropIndex(
                name: "IX_Resource_ScopedLookup",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropIndex(
                name: "IX_Scope_Key",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropIndex(
                name: "IX_Scope_ParentId",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropIndex(
                name: "IX_Scope_ResourcePath",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropIndex(
                name: "IX_PermissionRule_CreatedAt",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.DropIndex(
                name: "IX_PermissionRule_CreatedBy",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.DropIndex(
                name: "IX_PermissionRule_Id",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.DropIndex(
                name: "IX_PermissionRule_ModifiedAt",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.DropIndex(
                name: "IX_PermissionRule_ModifiedBy",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "Icon",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "Key",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "OwnerOrganizationUnitId",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "OwnerPersonId",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "OwnerPositionId",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "ParentId",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "ResourcePath",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "hasFieldBaseCondition",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "hasRelationBaseCondition",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "hasScope",
                schema: "authorization",
                table: "Scope");

            migrationBuilder.DropColumn(
                name: "JoinDetailId",
                schema: "authorization",
                table: "PermissionRule");

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "authorization",
                table: "Scope",
                newName: "scope");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                schema: "authorization",
                table: "Scope",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "authorization",
                table: "Scope",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "authorization",
                table: "Scope",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<Guid>(
                name: "PermissionId",
                schema: "authorization",
                table: "Scope",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                schema: "authorization",
                table: "PermissionRule",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "authorization",
                table: "PermissionRule",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "authorization",
                table: "PermissionRule",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "JoinEntity",
                schema: "authorization",
                table: "PermissionRule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JoinField",
                schema: "authorization",
                table: "PermissionRule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JoinForeignKey",
                schema: "authorization",
                table: "PermissionRule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JoinLocalKey",
                schema: "authorization",
                table: "PermissionRule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Type",
                schema: "authorization",
                table: "PermissionRule",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "Resources",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Category = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Key = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResourcePath = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    hasFieldBaseCondition = table.Column<bool>(type: "bit", nullable: false),
                    hasRelationBaseCondition = table.Column<bool>(type: "bit", nullable: false),
                    hasScope = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_Resources_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "authorization",
                        principalTable: "Resources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Scope_PermissionId",
                schema: "authorization",
                table: "Scope",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_CreatedAt",
                schema: "authorization",
                table: "Resources",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_CreatedBy",
                schema: "authorization",
                table: "Resources",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_ModifiedAt",
                schema: "authorization",
                table: "Resources",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_ModifiedBy",
                schema: "authorization",
                table: "Resources",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_OwnerOrgUnit",
                schema: "authorization",
                table: "Resources",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_OwnerPerson",
                schema: "authorization",
                table: "Resources",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Resource_ScopedLookup",
                schema: "authorization",
                table: "Resources",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Key",
                schema: "authorization",
                table: "Resources",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ParentId",
                schema: "authorization",
                table: "Resources",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ResourcePath",
                schema: "authorization",
                table: "Resources",
                column: "ResourcePath");

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
                name: "FK_Scope_Permissions_PermissionId",
                schema: "authorization",
                table: "Scope",
                column: "PermissionId",
                principalSchema: "authorization",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
