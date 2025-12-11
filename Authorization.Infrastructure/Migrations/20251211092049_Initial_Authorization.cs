using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Authorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "authorization");

            migrationBuilder.CreateTable(
                name: "Resources",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Route = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_Resources_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "authorization",
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataScopes",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssigneeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssigneeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SpecificUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomFilter = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Depth = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssigneeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssigneeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsAllow = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Condition = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalSchema: "authorization",
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Permission_CreatedAt",
                schema: "authorization",
                table: "Permissions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_CreatedBy",
                schema: "authorization",
                table: "Permissions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_ModifiedAt",
                schema: "authorization",
                table: "Permissions",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_ModifiedBy",
                schema: "authorization",
                table: "Permissions",
                column: "ModifiedBy");

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
                name: "IX_Permissions_Resource",
                schema: "authorization",
                table: "Permissions",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Resource_Assignee_Action",
                schema: "authorization",
                table: "Permissions",
                columns: new[] { "ResourceId", "AssigneeType", "AssigneeId", "Action" },
                unique: true);

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
                name: "IX_Resources_Key",
                schema: "authorization",
                table: "Resources",
                column: "Key",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataScopes",
                schema: "authorization");

            migrationBuilder.DropTable(
                name: "Permissions",
                schema: "authorization");

            migrationBuilder.DropTable(
                name: "Resources",
                schema: "authorization");
        }
    }
}
