using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit1_Authorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Resources_ParentId",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropTable(
                name: "DataScopes",
                schema: "authorization");

            migrationBuilder.DropIndex(
                name: "IX_Resources_Category_Active",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_Id",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_Parent_Order",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_Path",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_Type_Active",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Action_Resource",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_ActivePriority",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Assignee",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Id",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Resource_Assignee_Action",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Path",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Condition",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsAllow",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Order",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.RenameIndex(
                name: "IX_Permissions_Resource",
                schema: "authorization",
                table: "Permissions",
                newName: "IX_Permissions_ResourceId");

            migrationBuilder.AlterColumn<byte>(
                name: "Type",
                schema: "authorization",
                table: "Resources",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Route",
                schema: "authorization",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "authorization",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                schema: "authorization",
                table: "Resources",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldUnicode: false,
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "authorization",
                table: "Resources",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                schema: "authorization",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                schema: "authorization",
                table: "Resources",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "authorization",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Category",
                schema: "authorization",
                table: "Resources",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerOrganizationUnitId",
                schema: "authorization",
                table: "Resources",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerPersonId",
                schema: "authorization",
                table: "Resources",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerPositionId",
                schema: "authorization",
                table: "Resources",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResourcePath",
                schema: "authorization",
                table: "Resources",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "authorization",
                table: "Permissions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "authorization",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "AssigneeType",
                schema: "authorization",
                table: "Permissions",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<byte>(
                name: "Action",
                schema: "authorization",
                table: "Permissions",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerOrganizationUnitId",
                schema: "authorization",
                table: "Permissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerPersonId",
                schema: "authorization",
                table: "Permissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerPositionId",
                schema: "authorization",
                table: "Permissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Scope",
                schema: "authorization",
                table: "Permissions",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecificScopeId",
                schema: "authorization",
                table: "Permissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Type",
                schema: "authorization",
                table: "Permissions",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

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
                name: "IX_Resources_ParentId",
                schema: "authorization",
                table: "Resources",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ResourcePath",
                schema: "authorization",
                table: "Resources",
                column: "ResourcePath");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_OwnerOrgUnit",
                schema: "authorization",
                table: "Permissions",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_OwnerPerson",
                schema: "authorization",
                table: "Permissions",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_ScopedLookup",
                schema: "authorization",
                table: "Permissions",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_FastLookup",
                schema: "authorization",
                table: "Permissions",
                columns: new[] { "AssigneeId", "ResourceId", "Action" });

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
                name: "FK_Resources_Resources_ParentId",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resource_OwnerOrgUnit",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resource_OwnerPerson",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resource_ScopedLookup",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_ParentId",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_ResourcePath",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Permission_OwnerOrgUnit",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permission_OwnerPerson",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permission_ScopedLookup",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_FastLookup",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "OwnerOrganizationUnitId",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "OwnerPersonId",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "OwnerPositionId",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ResourcePath",
                schema: "authorization",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "OwnerOrganizationUnitId",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "OwnerPersonId",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "OwnerPositionId",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Scope",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "SpecificScopeId",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "authorization",
                table: "Permissions");

            migrationBuilder.RenameIndex(
                name: "IX_Permissions_ResourceId",
                schema: "authorization",
                table: "Permissions",
                newName: "IX_Permissions_Resource");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                schema: "authorization",
                table: "Resources",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<string>(
                name: "Route",
                schema: "authorization",
                table: "Resources",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "authorization",
                table: "Resources",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                schema: "authorization",
                table: "Resources",
                type: "varchar(200)",
                unicode: false,
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "authorization",
                table: "Resources",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                schema: "authorization",
                table: "Resources",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                schema: "authorization",
                table: "Resources",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "authorization",
                table: "Resources",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                schema: "authorization",
                table: "Resources",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<string>(
                name: "Path",
                schema: "authorization",
                table: "Resources",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "authorization",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "authorization",
                table: "Permissions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AssigneeType",
                schema: "authorization",
                table: "Permissions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                schema: "authorization",
                table: "Permissions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                schema: "authorization",
                table: "Permissions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAllow",
                schema: "authorization",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                schema: "authorization",
                table: "Permissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "authorization",
                table: "Permissions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "DataScopes",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssigneeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssigneeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CustomFilter = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Depth = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Scope = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SpecificUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataScopes_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalSchema: "authorization",
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Category_Active",
                schema: "authorization",
                table: "Resources",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Id",
                schema: "authorization",
                table: "Resources",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Parent_Order",
                schema: "authorization",
                table: "Resources",
                columns: new[] { "ParentId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Path",
                schema: "authorization",
                table: "Resources",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Type_Active",
                schema: "authorization",
                table: "Resources",
                columns: new[] { "Type", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Action_Resource",
                schema: "authorization",
                table: "Permissions",
                columns: new[] { "Action", "ResourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ActivePriority",
                schema: "authorization",
                table: "Permissions",
                columns: new[] { "AssigneeId", "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Assignee",
                schema: "authorization",
                table: "Permissions",
                columns: new[] { "AssigneeType", "AssigneeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Id",
                schema: "authorization",
                table: "Permissions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Resource_Assignee_Action",
                schema: "authorization",
                table: "Permissions",
                columns: new[] { "ResourceId", "AssigneeType", "AssigneeId", "Action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataScope_CreatedAt",
                schema: "authorization",
                table: "DataScopes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DataScope_CreatedBy",
                schema: "authorization",
                table: "DataScopes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DataScope_ModifiedAt",
                schema: "authorization",
                table: "DataScopes",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DataScope_ModifiedBy",
                schema: "authorization",
                table: "DataScopes",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DataScopes_Active",
                schema: "authorization",
                table: "DataScopes",
                columns: new[] { "AssigneeId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DataScopes_Assignee",
                schema: "authorization",
                table: "DataScopes",
                columns: new[] { "AssigneeType", "AssigneeId" });

            migrationBuilder.CreateIndex(
                name: "IX_DataScopes_Id",
                schema: "authorization",
                table: "DataScopes",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataScopes_Resource",
                schema: "authorization",
                table: "DataScopes",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataScopes_Resource_Assignee",
                schema: "authorization",
                table: "DataScopes",
                columns: new[] { "ResourceId", "AssigneeType", "AssigneeId" });

            migrationBuilder.CreateIndex(
                name: "IX_DataScopes_Scope_SpecificUnit",
                schema: "authorization",
                table: "DataScopes",
                columns: new[] { "Scope", "SpecificUnitId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Resources_ParentId",
                schema: "authorization",
                table: "Resources",
                column: "ParentId",
                principalSchema: "authorization",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
