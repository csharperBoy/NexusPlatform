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
                name: "OutboxMessages",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AssemblyQualifiedName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ErrorStackTrace = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    EventVersion = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Key = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Category = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResourcePath = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    hasScope = table.Column<bool>(type: "bit", nullable: false),
                    hasFieldBaseCondition = table.Column<bool>(type: "bit", nullable: false),
                    hasRelationBaseCondition = table.Column<bool>(type: "bit", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AssigneeType = table.Column<byte>(type: "tinyint", nullable: false),
                    AssigneeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<byte>(type: "tinyint", nullable: false),
                    Effect = table.Column<byte>(type: "tinyint", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "PermissionRule",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JoinEntity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JoinLocalKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JoinForeignKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JoinField = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Operator = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogicalOperator = table.Column<byte>(type: "tinyint", nullable: false),
                    GroupOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionRule_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "authorization",
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scope",
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
                    table.PrimaryKey("PK_Scope", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scope_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "authorization",
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                schema: "authorization",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_OccurredOnUtc",
                schema: "authorization",
                table: "OutboxMessages",
                columns: new[] { "Status", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_TypeName",
                schema: "authorization",
                table: "OutboxMessages",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRule_PermissionId",
                schema: "authorization",
                table: "PermissionRule",
                column: "PermissionId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Unique",
                schema: "authorization",
                table: "Permissions",
                columns: new[] { "ResourceId", "Action", "AssigneeType", "AssigneeId", "Effect" },
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

            migrationBuilder.CreateIndex(
                name: "IX_Scope_PermissionId",
                schema: "authorization",
                table: "Scope",
                column: "PermissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "authorization");

            migrationBuilder.DropTable(
                name: "PermissionRule",
                schema: "authorization");

            migrationBuilder.DropTable(
                name: "Scope",
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
